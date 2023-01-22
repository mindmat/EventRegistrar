using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Reductions;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public RegistrationController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpGet("api/registrationforms/{formExternalIdentifier}/RegistrationExternalIdentifiers")]
    public Task<IEnumerable<string>> GetAllExternalRegistrationIdentifiers(string formExternalIdentifier)
    {
        return _mediator.Send(new AllExternalRegistrationIdentifiersQuery { RegistrationFormExternalIdentifier = formExternalIdentifier });
    }

    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/swapFirstLastName")]
    public async Task SwapFirstLastName(string eventAcronym, Guid registrationId)
    {
        await _mediator.Send(new SwapFirstLastNameCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationId = registrationId
                             });
    }
}