using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms;

public class RegistrationFormController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public RegistrationFormController(IMediator mediator,
                                      IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpPost("api/events/{eventAcronym}/registrationForms/{formId}")]
    public async Task SaveRegistrationFormDefinition(string eventAcronym, string formId)
    {
        await _mediator.Send(new SaveRegistrationFormDefinitionCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 FormId = formId
                             });
    }

    [HttpGet("api/events/{eventAcronym}/registrationForms/pending")]
    public async Task<IEnumerable<RegistrationFormItem>> GetPendingRegistrationForms(string eventAcronym)
    {
        return await _mediator.Send(new PendingRegistrationFormQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpPost("api/events/{eventAcronym}/registrationForms/{formId}/mappings")]
    public async Task SaveRegistrationFormMappings(string eventAcronym,
                                                   Guid formId,
                                                   [FromBody] RegistrationFormGroup form)
    {
        await _mediator.Send(new SaveRegistrationFormMappingsCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 FormId = formId,
                                 Mappings = form
                             });
    }

    [HttpDelete("api/events/{eventAcronym}/registrationForms/{registrationFormId}")]
    public async Task DeleteRegistrationForm(string eventAcronym, Guid registrationFormId)
    {
        await _mediator.Send(new DeleteRegistrationFormCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationFormId = registrationFormId
                             });
    }


    [HttpGet("api/events/{eventAcronym}/formPaths")]
    public async Task<IEnumerable<RegistrationFormGroup>> GetFormPaths(string eventAcronym)
    {
        return await _mediator.Send(new FormPathsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/availableQuestionOptionMappings")]
    public async Task<IEnumerable<AvailableQuestionOptionMapping>> AvailableQuestionOptionMappingsQuery(
        string eventAcronym)
    {
        return await _mediator.Send(new AvailableQuestionOptionMappingsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/availableQuestionMappings")]
    public async Task<IEnumerable<AvailableQuestionMapping>> AvailableQuestionMappingsQuery(string eventAcronym)
    {
        return await _mediator.Send(new AvailableQuestionMappingsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }
}