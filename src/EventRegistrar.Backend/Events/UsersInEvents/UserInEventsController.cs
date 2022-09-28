using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UserInEventsController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public UserInEventsController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    //[HttpGet("api/me/events")]
    //public Task<IEnumerable<UserInEventDisplayItem>> GetMyEvents(bool includeRequestedEvents)
    //{
    //    return _mediator.Send(new EventsOfUserQuery { IncludeRequestedEvents = includeRequestedEvents });
    //}
}