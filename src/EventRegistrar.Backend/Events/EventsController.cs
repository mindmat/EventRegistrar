using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events;

public class EventsController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator,
                            IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }


    [HttpPost("api/events/{eventAcronym}/openRegistration")]
    public async Task OpenRegistration(string eventAcronym, bool deleteTestData = false)
    {
        await _mediator.Send(new OpenRegistrationCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 DeleteTestData = deleteTestData
                             });
    }
}