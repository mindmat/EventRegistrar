using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;

using MediatR;

using Newtonsoft.Json;

using QuestionType = EventRegistrar.Backend.RegistrationForms.Questions.QuestionType;

namespace EventRegistrar.Backend.Registrations.Register;

public class ProcessRawRegistrationCommand : IRequest
{
    public Guid RawRegistrationId { get; set; }
}

public class ProcessRawRegistrationCommandHandler : IRequestHandler<ProcessRawRegistrationCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IQueryable<RegistrationForm> _forms;
    private readonly ILogger _logger;
    private readonly IRepository<RawRegistration> _rawRegistrations;
    private readonly RegistrationProcessorDelegator _registrationProcessorDelegator;
    private readonly IRepository<Registration> _registrations;
    private readonly IRepository<Response> _responses;

    public ProcessRawRegistrationCommandHandler(ILogger logger,
                                                IRepository<RawRegistration> rawRegistrations,
                                                IRepository<Registration> registrations,
                                                IRepository<Response> responses,
                                                IQueryable<RegistrationForm> forms,
                                                RegistrationProcessorDelegator registrationProcessorDelegator,
                                                IEventBus eventBus)
    {
        _logger = logger;
        _rawRegistrations = rawRegistrations;
        _registrations = registrations;
        _responses = responses;
        _forms = forms;
        _registrationProcessorDelegator = registrationProcessorDelegator;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ProcessRawRegistrationCommand command, CancellationToken cancellationToken)
    {
        var rawRegistration =
            await _rawRegistrations.FirstOrDefaultAsync(reg => reg.Id == command.RawRegistrationId, cancellationToken);
        if (rawRegistration == null)
        {
            throw new KeyNotFoundException($"Invalid RawRegistrationId received {command.RawRegistrationId}");
        }

        var googleRegistration =
            JsonConvert.DeserializeObject<RegistrationForms.GoogleForms.Registration>(rawRegistration.ReceivedMessage);

        var form = await _forms.Where(frm => frm.ExternalIdentifier == rawRegistration.FormExternalIdentifier)
                               .Include(frm => frm.Questions)
                               .ThenInclude(qst => qst.QuestionOptions)
                               .FirstOrDefaultAsync(cancellationToken);
        if (form == null)
        {
            throw new KeyNotFoundException($"No form found with id '{rawRegistration.FormExternalIdentifier}'");
        }

        _logger.LogInformation(
            $"Questions: {form.Questions?.Count}, Options: {form.Questions?.Sum(qst => qst.QuestionOptions?.Count)}");

        // check form state
        if (form.State == State.RegistrationClosed)
        {
            throw new ApplicationException("Registration is closed");
        }
        //if (!form.EventId.HasValue)
        //{
        //    throw new ApplicationException("Registration form is not yet assigned to an event");
        //}

        var registration = await _registrations.FirstOrDefaultAsync(
                               reg => reg.ExternalIdentifier == rawRegistration.RegistrationExternalIdentifier, cancellationToken);
        if (registration != null)
        {
            throw new ApplicationException(
                $"Registration with external identifier '{rawRegistration.RegistrationExternalIdentifier}' already exists");
        }

        registration = new Registration
                       {
                           Id = Guid.NewGuid(),
                           EventId = form.EventId,
                           ExternalIdentifier = rawRegistration.RegistrationExternalIdentifier,
                           RegistrationFormId = form.Id,
                           ReceivedAt = DateTime.UtcNow,
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
                    await _responses.InsertOrUpdateEntity(response, cancellationToken);
                }
            }
            else
            {
                var response = new Response
                               {
                                   Id = Guid.NewGuid(),
                                   RegistrationId = registration.Id,
                                   ResponseString = string.IsNullOrEmpty(rawResponse.Response)
                                                        ? string.Join(", ", rawResponse.Responses)
                                                        : rawResponse.Response,
                                   QuestionId = responseLookup.questionId
                               };
                registration.Responses.Add(response);
                await _responses.InsertOrUpdateEntity(response, cancellationToken);
            }
        }

        var spots = await _registrationProcessorDelegator.Process(registration);
        _eventBus.Publish(new RegistrationProcessed
                          {
                              Id = Guid.NewGuid(),
                              EventId = form.EventId,
                              RegistrationId = registration.Id,
                              FirstName = registration.RespondentFirstName,
                              LastName = registration.RespondentLastName,
                              Email = registration.RespondentEmail,
                              Registrables = spots.Select(spt => spt.Registrable?.Name).ToArray()
                          });

        return Unit.Value;
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