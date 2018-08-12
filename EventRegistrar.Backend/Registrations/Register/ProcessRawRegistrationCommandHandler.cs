using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Seats;
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
        private readonly ImbalanceManager _imbalanceManager;
        private readonly ILogger _logger;
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _optionToRegistrableMappings;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly PriceCalculator _priceCalculator;
        private readonly IRepository<RawRegistration> _rawRegistrations;
        private readonly IRepository<Registration> _registrations;
        private readonly IRepository<Response> _responses;
        private readonly IRepository<Seat> _seats;

        public ProcessRawRegistrationCommandHandler(ILogger logger,
                                                    IRepository<RawRegistration> rawRegistrations,
                                                    IRepository<Registration> registrations,
                                                    IRepository<Seat> seats,
                                                    IRepository<Response> responses,
                                                    IQueryable<QuestionOptionToRegistrableMapping> optionToRegistrableMappings,
                                                    IQueryable<RegistrationForm> forms,
                                                    PhoneNormalizer phoneNormalizer,
                                                    PriceCalculator priceCalculator,
                                                    ImbalanceManager imbalanceManager)
        {
            _logger = logger;
            _rawRegistrations = rawRegistrations;
            _registrations = registrations;
            _seats = seats;
            _responses = responses;
            _optionToRegistrableMappings = optionToRegistrableMappings;
            _forms = forms;
            _phoneNormalizer = phoneNormalizer;
            _priceCalculator = priceCalculator;
            _imbalanceManager = imbalanceManager;
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
                                    .Include(frm => frm.Questions.Select(qst => qst.QuestionOptions))
                                    .Include(frm => frm.Questions)
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
                State = RegistrationState.Received
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
            await _registrations.InsertOrUpdateEntity(registration, cancellationToken);

            var processConfiguration = form.ProcessConfigurationJson != null
                ? JsonConvert.DeserializeObject<IEnumerable<IRegistrationProcessConfiguration>>(form.ProcessConfigurationJson)
                : GetHardcodedConfiguration(form.Id);

            //if (form.QuestionId_FirstName.HasValue &&
            //    responseLookup.questionId == form.QuestionId_FirstName)
            //{
            //    registration.RespondentFirstName = rawResponse.Response;
            //}
            //if (form.QuestionId_LastName.HasValue &&
            //    responseLookup.questionId == form.QuestionId_LastName)
            //{
            //    registration.RespondentLastName = rawResponse.Response;
            //}
            //if (form.QuestionId_Remarks.HasValue &&
            //    responseLookup.questionId == form.QuestionId_Remarks)
            //{
            //    registration.Remarks = rawResponse.Response;
            //}
            //if (form.QuestionId_Phone.HasValue &&
            //    responseLookup.questionId == form.QuestionId_Phone)
            //{
            //    registration.Phone = rawResponse.Response;
            //}

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
            var ownSeats = new List<Seat>();

            var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
            var registrables = await _optionToRegistrableMappings
                                            .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
                                            .Include(map => map.Registrable)
                                            .Include(map => map.Registrable.Seats)
                                            .ToListAsync(cancellationToken);
            var registrableIds_CheckWaitingList = new List<Guid>();
            foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
            {
                foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                {
                    var partnerEmail = registrable.QuestionId_PartnerEmail.HasValue
                        ? registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == registrable.QuestionId_PartnerEmail.Value)?.ResponseString
                        : null;
                    var isLeader = registrable.QuestionOptionId_Leader.HasValue &&
                                   registration.Responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Leader.Value);
                    var isFollower = registrable.QuestionOptionId_Follower.HasValue &&
                                     registration.Responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Follower.Value);
                    var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
                    var seat = ReserveSeat(registration.EventId, registrable.Registrable, response, registration.RespondentEmail, partnerEmail, role, out Guid? registrableId_CheckWaitingList);
                    if (registrableId_CheckWaitingList != null)
                    {
                        registrableIds_CheckWaitingList.Add(registrableId_CheckWaitingList.Value);
                    }
                    if (seat == null)
                    {
                        registration.SoldOutMessage = (registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
                                                      string.Format(Resources.RegistrableSoldOut, registrable.Registrable.Name);
                    }
                    else
                    {
                        ownSeats.Add(seat);
                    }
                }
            }
            registration.IsWaitingList = ownSeats.Any(seat => seat.IsWaitingList);
            if (registration.IsWaitingList == false && !registration.AdmittedAt.HasValue)
            {
                registration.AdmittedAt = DateTime.UtcNow;
            }
            registration.PhoneNormalized = _phoneNormalizer.NormalizePhone(registration.Phone);

            registration.Price = await _priceCalculator.CalculatePrice(registration);

            // ToDo: send mail
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

        private static void ComplementExistingSeat(Guid registrationId, Role ownRole, Seat existingSeat)
        {
            if (ownRole == Role.Leader && !existingSeat.RegistrationId.HasValue)
            {
                existingSeat.RegistrationId = registrationId;
            }
            else if (ownRole == Role.Follower && !existingSeat.RegistrationId_Follower.HasValue)
            {
                existingSeat.RegistrationId_Follower = registrationId;
            }
            else
            {
                throw new Exception($"Unexpected situation: Own Role {ownRole}, partner seat registrationId {existingSeat.RegistrationId}/registrationId_Follower {existingSeat.RegistrationId_Follower}");
            }
        }

        private static Seat FindMatchingSingleSeat(IEnumerable<Seat> seats, Role ownRole)
        {
            return seats?.FirstOrDefault(seat => string.IsNullOrEmpty(seat.PartnerEmail) &&
                                                 !seat.IsWaitingList &&
                                                 (ownRole == Role.Leader && !seat.RegistrationId.HasValue ||
                                                  ownRole == Role.Follower && !seat.RegistrationId_Follower.HasValue));
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

        private Seat FindPartnerSeat(Guid? eventId, string ownEmail, string partnerEmail, Role ownRole, ICollection<Seat> existingSeats, IQueryable<Registration> registrations)
        {
            var partnerSeats = existingSeats.Where(seat => seat.PartnerEmail == ownEmail).ToList();
            if (!partnerSeats.Any())
            {
                return null;
            }
            var otherRole = ownRole == Role.Leader ? Role.Follower : Role.Leader;
            var partnerRegistrationIds = partnerSeats.Select(seat => otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower).ToList();
            var partnerRegistrationThatReferenceThisEmail = registrations.Where(reg => (!eventId.HasValue || reg.RegistrationForm.EventId == eventId.Value) &&
                                                                                       partnerRegistrationIds.Contains(reg.Id))
                                                                         .ToList();
            _logger.LogInformation($"Partner registrations with this partner mail: {string.Join(", ", partnerRegistrationThatReferenceThisEmail.Select(reg => reg.Id))}");
            var partnerRegistrationId = partnerRegistrationThatReferenceThisEmail.FirstOrDefault(reg => reg.RespondentEmail == partnerEmail)?.Id;
            return partnerSeats.FirstOrDefault(seat => partnerRegistrationId == (otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower));
        }

        private IEnumerable<IRegistrationProcessConfiguration> GetHardcodedConfiguration(Guid formId)
        {
            //if (formId == )
            {
                return new[]
                {
                    new SingleRegistrationProcessConfiguration
                    {
                    },
                };
            }
        }

        private Seat ReserveSeat(Guid? eventId,
                                 Registrable registrable,
                                 Response response,
                                 string ownEmail,
                                 string partnerEmail,
                                 Role? role,
                                 out Guid? registrableId_CheckWaitingList)
        {
            Seat seat;
            registrableId_CheckWaitingList = null;
            var seats = registrable.Seats.Where(st => !st.IsCancelled).ToList();
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var waitingList = seats.Any(st => st.IsWaitingList);
                var seatAvailable = !waitingList && seats.Count < registrable.MaximumSingleSeats.Value;
                _logger.LogInformation($"Registrable {registrable.Name}, Seat count {seats.Count}, MaximumSingleSeats {registrable.MaximumSingleSeats}, seat available {seatAvailable}");
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }
                seat = new Seat
                {
                    FirstPartnerJoined = DateTime.UtcNow,
                    RegistrationId = response.RegistrationId,
                    RegistrableId = registrable.Id,
                    IsWaitingList = !seatAvailable
                };
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
                if (!role.HasValue)
                {
                    throw new Exception("No role found");
                }
                var isPartnerRegistration = !string.IsNullOrEmpty(partnerEmail);
                var ownRole = role.Value;
                var waitingList = seats.Where(st => st.IsWaitingList).ToList();
                if (isPartnerRegistration)
                {
                    // complement existing partner seat
                    var existingPartnerSeat = FindPartnerSeat(eventId, ownEmail, partnerEmail, ownRole, seats, _registrations);

                    if (existingPartnerSeat != null)
                    {
                        ComplementExistingSeat(response.RegistrationId, ownRole, existingPartnerSeat);
                        return existingPartnerSeat;
                    }

                    // create new partner seat
                    var waitingListForPartnerRegistrations = waitingList.Any(st => !string.IsNullOrEmpty(st.PartnerEmail));
                    var seatAvailable = !waitingListForPartnerRegistrations && seats.Count < registrable.MaximumDoubleSeats.Value;
                    if (!seatAvailable && !registrable.HasWaitingList)
                    {
                        return null;
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
                        PartnerEmail = partnerEmail,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
                        RegistrableId = registrable.Id,
                        IsWaitingList = !seatAvailable
                    };
                }
                else
                {
                    // single registration
                    var waitingListForSingleLeaders = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId.HasValue);
                    var waitingListForSingleFollowers = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId_Follower.HasValue);

                    var waitingListForOwnRole = ownRole == Role.Leader && waitingListForSingleLeaders ||
                                                ownRole == Role.Follower && waitingListForSingleFollowers;
                    var matchingSingleSeat = FindMatchingSingleSeat(seats, ownRole);
                    var seatAvailable = !waitingListForOwnRole && (_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole) || matchingSingleSeat != null);
                    if (!seatAvailable && !registrable.HasWaitingList)
                    {
                        return null;
                    }
                    if (ownRole == Role.Leader && waitingListForSingleFollowers ||
                        ownRole == Role.Follower && waitingListForSingleLeaders)
                    {
                        registrableId_CheckWaitingList = registrable.Id;
                    }
                    if (!waitingListForOwnRole && matchingSingleSeat != null)
                    {
                        ComplementExistingSeat(response.RegistrationId, ownRole, matchingSingleSeat);
                        return matchingSingleSeat;
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
                        RegistrableId = registrable.Id,
                        IsWaitingList = !seatAvailable
                    };
                }
            }
            else
            {
                // no limit
                seat = new Seat
                {
                    RegistrationId = response.RegistrationId,
                    RegistrableId = registrable.Id,
                    FirstPartnerJoined = DateTime.UtcNow
                };
            }

            seat.Id = Guid.NewGuid();
            _seats.InsertOrUpdateEntity(seat);

            return seat;
        }
    }
}