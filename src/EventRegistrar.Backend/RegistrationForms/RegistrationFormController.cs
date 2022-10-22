using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;

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
}