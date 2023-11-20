using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events;

public class EventsController(IMediator mediator,
                              IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpPost("api/events/{eventAcronym}/openRegistration")]
    public async Task OpenRegistration(string eventAcronym, bool deleteTestData = false)
    {
        await mediator.Send(new OpenRegistrationCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                DeleteTestData = deleteTestData
                            });
    }
}