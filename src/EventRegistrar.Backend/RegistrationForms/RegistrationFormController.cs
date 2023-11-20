using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.RegistrationForms;

public class RegistrationFormController(IMediator mediator,
                                        IEventAcronymResolver eventAcronymResolver)
    : Controller
{
    [HttpDelete("api/events/{eventAcronym}/registrationForms/{registrationFormId}")]
    public async Task DeleteRegistrationForm(string eventAcronym, Guid registrationFormId)
    {
        await mediator.Send(new DeleteRegistrationFormCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationFormId = registrationFormId
                            });
    }


    [HttpGet("api/events/{eventAcronym}/formPaths")]
    public async Task<IEnumerable<RegistrationFormGroup>> GetFormPaths(string eventAcronym)
    {
        return await mediator.Send(new FormPathsQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                   });
    }
}