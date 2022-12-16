using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Matching;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;

using Newtonsoft.Json;

using QuestionType = EventRegistrar.Backend.RegistrationForms.Questions.QuestionType;

namespace EventRegistrar.Backend.Registrations.Register;

public class ProcessRawRegistrationCommand : IRequest
{
    public Guid RawRegistrationId { get; set; }
}

public class ProcessRawRegistrationCommandHandler : AsyncRequestHandler<ProcessRawRegistrationCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IQueryable<RegistrationForm> _forms;
    private readonly ILogger _logger;
    private readonly IRepository<RawRegistration> _rawRegistrations;
    private readonly RegistrationProcessorDelegator _registrationProcessorDelegator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRepository<Registration> _registrations;
    private readonly IRepository<Response> _responses;

    public ProcessRawRegistrationCommandHandler(ILogger logger,
                                                IRepository<RawRegistration> rawRegistrations,
                                                IRepository<Registration> registrations,
                                                IRepository<Response> responses,
                                                IQueryable<RegistrationForm> forms,
                                                RegistrationProcessorDelegator registrationProcessorDelegator,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventBus eventBus)
    {
        _logger = logger;
        _rawRegistrations = rawRegistrations;
        _registrations = registrations;
        _responses = responses;
        _forms = forms;
        _registrationProcessorDelegator = registrationProcessorDelegator;
        _dateTimeProvider = dateTimeProvider;
        _eventBus = eventBus;
    }

    protected override async Task Handle(ProcessRawRegistrationCommand command, CancellationToken cancellationToken)
    {
        var rawRegistration = await _rawRegistrations.AsTracking()
                                                     .FirstAsync(reg => reg.Id == command.RawRegistrationId,
                                                                 cancellationToken);
        if (rawRegistration.Processed != null)
        {
            throw new KeyNotFoundException($"RawRegistrationId {command.RawRegistrationId} has already been processed at {rawRegistration.Processed}");
        }

        try
        {
            var googleRegistration = JsonConvert.DeserializeObject<RegistrationForms.GoogleForms.Registration>(rawRegistration.ReceivedMessage);

            var form = await _forms.Where(frm => frm.ExternalIdentifier == rawRegistration.FormExternalIdentifier)
                                   .Include(frm => frm.Questions!)
                                   .ThenInclude(qst => qst.QuestionOptions)
                                   .FirstOrDefaultAsync(cancellationToken);
            if (form == null)
            {
                throw new KeyNotFoundException($"No form found with id '{rawRegistration.FormExternalIdentifier}'");
            }

            _logger.LogInformation($"Questions: {form.Questions?.Count}, Options: {form.Questions?.Sum(qst => qst.QuestionOptions?.Count)}");

            // check form state
            if (form.State == EventState.RegistrationClosed)
            {
                throw new ApplicationException("Registration is closed");
            }
            //if (!form.EventId.HasValue)
            //{
            //    throw new ApplicationException("Registration form is not yet assigned to an event");
            //}

            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.ExternalIdentifier == rawRegistration.RegistrationExternalIdentifier, cancellationToken);
            if (registration != null)
            {
                // Duplicate RawRegistration?
                var processedDuplicate = await _rawRegistrations.Where(rrg => rrg.EventAcronym == rawRegistration.EventAcronym
                                                                           && rrg.FormExternalIdentifier == rawRegistration.FormExternalIdentifier
                                                                           && rrg.RegistrationExternalIdentifier == rawRegistration.RegistrationExternalIdentifier
                                                                           && rrg.Processed != null)
                                                                .FirstOrDefaultAsync(cancellationToken);
                if (processedDuplicate != null
                 && processedDuplicate.ReceivedMessage == rawRegistration.ReceivedMessage)
                {
                    _rawRegistrations.Remove(rawRegistration);
                    _eventBus.Publish(new QueryChanged
                                      {
                                          EventId = form.EventId,
                                          QueryName = nameof(UnprocessedRawRegistrationCountQuery)
                                      });
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
                               ReceivedAt = _dateTimeProvider.Now,
                               ExternalTimestamp = googleRegistration.Timestamp,
                               RespondentEmail = googleRegistration.Email,
                               //Language = form.Language,
                               State = RegistrationState.Received,
                               Responses = new List<Response>()
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
                                                                : rawResponse.Response,
                                           QuestionId = responseLookup.questionId,
                                           QuestionOptionId = questionOptionId
                                       };
                        registration.Responses.Add(response);
                        _responses.InsertObjectTree(response);
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
                    _responses.InsertObjectTree(response);
                }
            }

            var spots = (await _registrationProcessorDelegator.Process(registration)).ToList();

            _eventBus.Publish(new RegistrationProcessed
                              {
                                  Id = Guid.NewGuid(),
                                  EventId = form.EventId,
                                  RegistrationId = registration.Id,
                                  FirstName = registration.RespondentFirstName,
                                  LastName = registration.RespondentLastName,
                                  Email = registration.RespondentEmail,
                                  Registrables = spots.Select(spt => spt.Registrable?.DisplayName ?? string.Empty).ToArray()
                              });
            _eventBus.Publish(new QueryChanged
                              {
                                  EventId = form.EventId,
                                  QueryName = nameof(UnprocessedRawRegistrationCountQuery)
                              });
            foreach (var trackId in spots.Select(spt => spt.RegistrableId))
            {
                _eventBus.Publish(new QueryChanged
                                  {
                                      EventId = form.EventId,
                                      QueryName = nameof(ParticipantsOfRegistrableQuery),
                                      RowId = trackId
                                  });
            }

            if ((registration.PartnerNormalized != null && registration.RegistrationId_Partner == null) || registration.RegistrationId_Partner != null)
            {
                _eventBus.Publish(new QueryChanged
                                  {
                                      EventId = form.EventId,
                                      QueryName = nameof(RegistrationsWithUnmatchedPartnerQuery)
                                  });
            }


            rawRegistration.Processed = _dateTimeProvider.Now;
        }
        catch (Exception ex)
        {
            rawRegistration.LastProcessingError = ex.Message;
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
        return (question?.Id, optionId.HasValue ? new[] { optionId.Value } : new Guid[] { });
    }
}