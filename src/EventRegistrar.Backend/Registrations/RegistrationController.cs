using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations.Raw;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationController(IMediator mediator,
                                    IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpGet("api/registrationforms/{formExternalIdentifier}/RegistrationExternalIdentifiers")]
    public Task<IEnumerable<string>> GetAllExternalRegistrationIdentifiers(string formExternalIdentifier)
    {
        return mediator.Send(new AllExternalRegistrationIdentifiersQuery { RegistrationFormExternalIdentifier = formExternalIdentifier });
    }

    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/swapFirstLastName")]
    public async Task SwapFirstLastName(string eventAcronym, Guid registrationId)
    {
        await mediator.Send(new SwapFirstLastNameCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationId = registrationId
                            });
    }
}