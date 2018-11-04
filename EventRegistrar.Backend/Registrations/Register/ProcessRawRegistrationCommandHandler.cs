using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuestionType = EventRegistrar.Backend.RegistrationForms.Questions.QuestionType;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class ProcessRawRegistrationCommandHandler : IRequestHandler<ProcessRawRegistrationCommand>
    {
        private readonly IQueryable<RegistrationForm> _forms;
        private readonly ILogger _logger;
        private readonly PriceCalculator _priceCalculator;
        private readonly IRepository<RawRegistration> _rawRegistrations;
        private readonly RegistrationProcessorDelegator _registrationProcessorDelegator;
        private readonly IRepository<Registration> _registrations;
        private readonly IRepository<Response> _responses;
        private readonly ServiceBusClient _serviceBusClient;

        public ProcessRawRegistrationCommandHandler(ILogger logger,
                                                    IRepository<RawRegistration> rawRegistrations,
                                                    IRepository<Registration> registrations,
                                                    IRepository<Response> responses,
                                                    IQueryable<RegistrationForm> forms,
                                                    PriceCalculator priceCalculator,
                                                    RegistrationProcessorDelegator registrationProcessorDelegator,
                                                    ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _rawRegistrations = rawRegistrations;
            _registrations = registrations;
            _responses = responses;
            _forms = forms;
            _priceCalculator = priceCalculator;
            _registrationProcessorDelegator = registrationProcessorDelegator;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ProcessRawRegistrationCommand command, CancellationToken cancellationToken)
        {
            var rawRegistration = await _rawRegistrations.FirstOrDefaultAsync(reg => reg.Id == command.RawRegistrationId, cancellationToken);
            if (rawRegistration == null)
            {
                throw new KeyNotFoundException($"Invalid RawRegistrationId received {command.RawRegistrationId}");
            }

            var googleRegistration = JsonConvert.DeserializeObject<RegistrationForms.GoogleForms.Registration>(rawRegistration.ReceivedMessage);

            //var saveEventTask = DomainEventPersistor.Log(new RegistrationReceived
            //{
            //    FormExternalIdentifier = formId,
            //    RegistrationExternalIdentifier = id,
            //    Registration = await req.Content.ReadAsStringAsync()
            //});

            //RegistrationRegistered registrationRegistered;

            var form = await _forms.Where(frm => frm.ExternalIdentifier == rawRegistration.FormExternalIdentifier)
                                   .Include(frm => frm.Questions).ThenInclude(qst => qst.QuestionOptions)
                                   .FirstOrDefaultAsync(cancellationToken);
            if (form == null)
            {
                throw new KeyNotFoundException($"No form found with id '{rawRegistration.FormExternalIdentifier}'");
            }
            _logger.LogInformation($"Questions: {form.Questions.Count}, Options: {form.Questions.Sum(qst => qst.QuestionOptions.Count)}");

            // check form state
            if (form.State == State.RegistrationClosed)
            {
                throw new ApplicationException("Registration is closed");
            }
            if (!form.EventId.HasValue)
            {
                throw new ApplicationException("Registration form is not yet assigned to an event");
            }

            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.ExternalIdentifier == rawRegistration.RegistrationExternalIdentifier, cancellationToken);
            if (registration != null)
            {
                throw new ApplicationException("Registration with id '{id}' already exists");
            }

            // ToDo: check for duplicates
            //var registrationWithSameEmail = await _registrations
            //                                             .FirstOrDefaultAsync(reg => reg.RegistrationForm.EventId == form.EventId &&
            //                                                                         reg.RespondentEmail == googleRegistration.Email &&
            //                                                                         reg.State != RegistrationState.Cancelled);
            //if (registrationWithSameEmail != null)
            //{
            //    // HACK: hardcoded
            //    var sendMailCommand = new SendMailCommand
            //    {
            //        MailId = Guid.NewGuid(),
            //        Subject = "Duplicate registration",
            //        ContentPlainText = $"Hello, you can only register once so we have to discard your later registration. Your first registration we received at {registrationWithSameEmail.ReceivedAt.ToLocalTime()} is still valid",
            //        Sender = new EmailAddress { Email = "noreply@leapinlindy.ch" },
            //        To = new[] { new EmailAddress { Email = googleRegistration.Email } }
            //    };
            //    await ServiceBusClient.SendEvent(sendMailCommand, SendMailCommandHandler.SendMailQueueName);
            //    return new HttpResponseMessage(HttpStatusCode.BadRequest)
            //    {
            //        Content = new StringContent($"Registration with mail '{googleRegistration.Email}' already exists")
            //    };
            //}

            registration = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = form.EventId.Value,
                ExternalIdentifier = rawRegistration.RegistrationExternalIdentifier,
                RegistrationFormId = form.Id,
                ReceivedAt = DateTime.UtcNow,
                ExternalTimestamp = googleRegistration.Timestamp,
                RespondentEmail = googleRegistration.Email,
                Language = form.Language,
                State = RegistrationState.Received,
                Responses = new List<Response>()
            };
            //var responses = new List<Response>();

            foreach (var rawResponse in googleRegistration.Responses)
            {
                var responseLookup = LookupResponse(rawResponse, form.Questions);
                if (responseLookup.questionOptionId.Any())
                {
                    foreach (var questionOptionId in responseLookup.questionOptionId)
                    {
                        var response = new Response
                        {
                            Id = Guid.NewGuid(),
                            RegistrationId = registration.Id,
                            ResponseString = string.IsNullOrEmpty(rawResponse.Response)
                                ? string.Join(", ", rawResponse.Responses)
                                : rawResponse.Response,
                            QuestionId = responseLookup.questionId,
                            QuestionOptionId = questionOptionId
                        };
                        registration.Responses.Add(response);
                        await _responses.InsertOrUpdateEntity(response, cancellationToken);
                    }
                }
                else
                {
                    var response = new Response
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = registration.Id,
                        ResponseString = string.IsNullOrEmpty(rawResponse.Response) ? string.Join(", ", rawResponse.Responses) : rawResponse.Response,
                        QuestionId = responseLookup.questionId,
                    };
                    registration.Responses.Add(response);
                    await _responses.InsertOrUpdateEntity(response, cancellationToken);
                }
            }
            var spots = await _registrationProcessorDelegator.Process(registration, form);

            //await context.SaveChangesAsync();

            //registrationRegistered = new RegistrationRegistered
            //{
            //    EventId = form.EventId,
            //    RegistrationId = registration.Id,
            //    Registration = registration
            //};
            //context.DomainEvents.Save(registrationRegistered, form.Id);

            //var registration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == @event.RegistrationId);
            //var responses = await _responses.Where(rsp => rsp.RegistrationId == @event.RegistrationId).ToListAsync();
            //var ownSeats = new List<Seat>();

            //var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
            //var registrables = await _optionToRegistrableMappings
            //                                .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
            //                                .Include(map => map.Registrable)
            //                                .Include(map => map.Registrable.Seats)
            //                                .ToListAsync(cancellationToken);
            //var registrableIds_CheckWaitingList = new List<Guid>();
            //foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
            //{
            //    foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
            //    {
            //        var partnerEmail = registrable.QuestionId_Partner.HasValue
            //            ? registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == registrable.QuestionId_Partner.Value)?.ResponseString
            //            : null;
            //        var isLeader = registrable.QuestionOptionId_Leader.HasValue &&
            //                       registration.Responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Leader.Value);
            //        var isFollower = registrable.QuestionOptionId_Follower.HasValue &&
            //                         registration.Responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Follower.Value);
            //        var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
            //        var seat = ReserveSeat(registration.EventId, registrable.Registrable, response, registration.RespondentEmail, partnerEmail, role, out Guid? registrableId_CheckWaitingList);
            //        if (registrableId_CheckWaitingList != null)
            //        {
            //            registrableIds_CheckWaitingList.Add(registrableId_CheckWaitingList.Value);
            //        }
            //        if (seat == null)
            //        {
            //            registration.SoldOutMessage = (registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
            //                                          string.Format(Resources.RegistrableSoldOut, registrable.Registrable.Name);
            //        }
            //        else
            //        {
            //            ownSeats.Add(seat);
            //        }
            //    }
            //}

            //await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registration.Id }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            //if (registrableIds_CheckWaitingList.Any() && @event.EventId.HasValue)
            //{
            //    foreach (var registrableId in registrableIds_CheckWaitingList)
            //    {
            //        await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand { RegistrableId = registrableId }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
            //    }
            //}
            return Unit.Value;
        }

        private static (Guid? questionId, IEnumerable<Guid> questionOptionId) LookupResponse(ResponseData response, IEnumerable<Question> questions)
        {
            var question = questions?.FirstOrDefault(qst => qst.ExternalId == response.QuestionExternalId);
            if (question?.Type == QuestionType.Checkbox && response.Responses.Any())
            {
                var optionIds = question.QuestionOptions?.Where(qop => response.Responses.Any(rsp => rsp == qop.Answer)).Select(qop => qop.Id).ToList();
                return (question.Id, optionIds);
            }
            var optionId = question?.QuestionOptions?.Where(qop => qop.Answer == response.Response).FirstOrDefault()?.Id;
            return (question?.Id, optionId.HasValue ? new[] { optionId.Value } : new Guid[] { });
        }
    }
}