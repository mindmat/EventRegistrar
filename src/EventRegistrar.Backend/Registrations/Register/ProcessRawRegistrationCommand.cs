using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Matching;
using EventRegistrar.Backend.Registrations.Overview;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Registrations.Register;

public class ProcessRawRegistrationCommand : IRequest
{
    public Guid RawRegistrationId { get; set; }
}

public class ProcessRawRegistrationCommandHandler(ILogger logger,
                                                  IRepository<RawRegistration> rawRegistrations,
                                                  IRepository<Registration> registrations,
                                                  IRepository<Response> responses,
                                                  IQueryable<RegistrationForm> forms,
                                                  RegistrationProcessorDelegator registrationProcessorDelegator,
                                                  IDateTimeProvider dateTimeProvider,
                                                  ChangeTrigger changeTrigger)
    : IRequestHandler<ProcessRawRegistrationCommand>
{
    public async Task Handle(ProcessRawRegistrationCommand command, CancellationToken cancellationToken)
    {
        var rawRegistration = await rawRegistrations.AsTracking()
                                                    .FirstAsync(reg => reg.Id == command.RawRegistrationId,
                                                                cancellationToken);
        Guid? eventId = null;
        if (rawRegistration.Processed != null)
        {
            throw new KeyNotFoundException($"RawRegistrationId {command.RawRegistrationId} has already been processed at {rawRegistration.Processed}");
        }

        try
        {
            var googleRegistration = JsonConvert.DeserializeObject<GoogleRegistration>(rawRegistration.ReceivedMessage);

            var form = await forms.Where(frm => frm.ExternalIdentifier == rawRegistration.FormExternalIdentifier)
                                  .Include(frm => frm.Questions!)
                                  .ThenInclude(qst => qst.QuestionOptions)
                                  .FirstOrDefaultAsync(cancellationToken);
            if (form == null)
            {
                throw new KeyNotFoundException($"No form found with id '{rawRegistration.FormExternalIdentifier}'");
            }

            eventId = form.EventId;
            logger.LogInformation($"Questions: {form.Questions?.Count}, Options: {form.Questions?.Sum(qst => qst.QuestionOptions?.Count)}");

            // check form state
            if (form.State == EventState.RegistrationClosed)
            {
                throw new ApplicationException("Registration is closed");
            }
            //if (!form.EventId.HasValue)
            //{
            //    throw new ApplicationException("Registration form is not yet assigned to an event");
            //}

            var registration = await registrations.FirstOrDefaultAsync(reg => reg.ExternalIdentifier == rawRegistration.RegistrationExternalIdentifier, cancellationToken);
            if (registration != null)
            {
                // Duplicate RawRegistration?
                var processedDuplicate = await rawRegistrations.Where(rrg => rrg.EventAcronym == rawRegistration.EventAcronym
                                                                          && rrg.FormExternalIdentifier == rawRegistration.FormExternalIdentifier
                                                                          && rrg.RegistrationExternalIdentifier == rawRegistration.RegistrationExternalIdentifier
                                                                          && rrg.Processed != null)
                                                               .FirstOrDefaultAsync(cancellationToken);
                if (processedDuplicate != null
                 && processedDuplicate.ReceivedMessage == rawRegistration.ReceivedMessage)
                {
                    rawRegistrations.Remove(rawRegistration);
                    changeTrigger.QueryChanged<UnprocessedRawRegistrationCountQuery>(form.EventId);
                    return;
                }

                throw new ApplicationException($"Registration with external identifier '{rawRegistration.RegistrationExternalIdentifier}' already exists");
            }

            registration = new Registration
                           {
                               Id = Guid.NewGuid(),
                               EventId = form.EventId,
                               ExternalIdentifier = rawRegistration.RegistrationExternalIdentifier,
                               RegistrationFormId = form.Id,
                               ReceivedAt = dateTimeProvider.Now,
                               ExternalTimestamp = googleRegistration.Timestamp,
                               RespondentEmail = googleRegistration.Email,
                               //Language = form.Language,
                               State = RegistrationState.Received,
                               Responses = new List<Response>(),
                               PricePackageIds_ManualFallback = new List<Guid>(),
                               PricePackageIds_Admitted = new List<Guid>()
                           };

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
                                                                : rawResponse.Response.Trim(),
                                           QuestionId = responseLookup.questionId,
                                           QuestionOptionId = questionOptionId
                                       };
                        registration.Responses.Add(response);
                        responses.InsertObjectTree(response);
                    }
                }
                else
                {
                    var response = new Response
                                   {
                                       Id = Guid.NewGuid(),
                                       RegistrationId = registration.Id,
                                       ResponseString = string.IsNullOrEmpty(rawResponse.Response)
                                                            ? rawResponse.Responses.StringJoin()
                                                            : rawResponse.Response,
                                       QuestionId = responseLookup.questionId
                                   };
                    registration.Responses.Add(response);
                    responses.InsertObjectTree(response);
                }
            }

            var spots = (await registrationProcessorDelegator.Process(registration)).ToList();

            changeTrigger.PublishEvent(new RegistrationProcessed
                                       {
                                           Id = Guid.NewGuid(),
                                           EventId = form.EventId,
                                           RegistrationId = registration.Id,
                                           FirstName = registration.RespondentFirstName?.Trim(),
                                           LastName = registration.RespondentLastName?.Trim(),
                                           Email = registration.RespondentEmail?.Trim(),
                                           Registrables = spots.Select(spt => spt.Registrable?.DisplayName ?? string.Empty).ToArray()
                                       });
            changeTrigger.QueryChanged<UnprocessedRawRegistrationCountQuery>(form.EventId);
            foreach (var trackId in spots.Select(spt => spt.RegistrableId))
            {
                changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(form.EventId, trackId);
            }

            if (registration is { PartnerNormalized: not null, RegistrationId_Partner: null } || registration.RegistrationId_Partner != null)
            {
                changeTrigger.QueryChanged<RegistrationsWithUnmatchedPartnerQuery>(form.EventId);
            }

            changeTrigger.EnqueueCommand(new RecalculatePriceAndWaitingListCommand { RegistrationId = registration.Id });
            changeTrigger.QueryChanged<PaymentOverviewQuery>(form.EventId);
            changeTrigger.QueryChanged<PricePackageOverviewQuery>(form.EventId);
            changeTrigger.QueryChanged<ParticipantsOfEventQuery>(form.EventId);
            changeTrigger.QueryChanged<EventSetupStateQuery>(form.EventId);
            rawRegistration.Processed = dateTimeProvider.Now;
        }
        catch (Exception ex)
        {
            rawRegistration.LastProcessingError = ex.Message;
            if (eventId != null)
            {
                changeTrigger.QueryChanged<EventSetupStateQuery>(eventId.Value);
            }
        }
    }

    private static (Guid? questionId, IEnumerable<Guid> questionOptionId) LookupResponse(
        ResponseData response,
        IEnumerable<Question> questions)
    {
        var question = questions?.FirstOrDefault(qst => qst.ExternalId == response.QuestionExternalId);
        if (question?.Type == QuestionType.Checkbox && response.Responses.Any())
        {
            var optionIds = question.QuestionOptions?.Where(qop => response.Responses.Any(rsp => rsp == qop.Answer))
                                    .Select(qop => qop.Id)
                                    .ToList();
            return (question.Id, optionIds);
        }

        var optionId = question?.QuestionOptions?.Where(qop => qop.Answer == response.Response).FirstOrDefault()?.Id;
        return (question?.Id, optionId.HasValue ? new[] { optionId.Value } : Array.Empty<Guid>());
    }
}