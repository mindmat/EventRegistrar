using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.Matching;

public class PartnerMatchingController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PartnerMatchingController(IMediator mediator,
                                     IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }


    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/unbind")]
    public async Task UnbindPartnerRegistration(string eventAcronym, Guid registrationId)
    {
        await _mediator.Send(new UnbindPartnerRegistrationCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationId = registrationId
                             });
    }
}