using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.Matching;

public class PartnerMatchingController(IMediator mediator,
                                       IEventAcronymResolver eventAcronymResolver)
    : Controller
{
    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/unbind")]
    public async Task UnbindPartnerRegistration(string eventAcronym, Guid registrationId)
    {
        await mediator.Send(new UnbindPartnerRegistrationCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationId = registrationId
                            });
    }
}