using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

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

    [HttpPut("api/events/{eventAcronym}")]
    public Task CreateEvent(string eventAcronym,
                            string name,
                            Guid id,
                            Guid? eventId_CopyFrom)
    {
        return _mediator.Send(new CreateEventCommand
                              {
                                  Acronym = eventAcronym,
                                  Id = id,
                                  EventId_CopyFrom = eventId_CopyFrom,
                                  Name = name
                              });
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

    [HttpPost("api/events/{eventAcronym}/requestAccess")]
    public async Task<Guid> RequestAccess(string eventAcronym)
    {
        return await _mediator.Send(new RequestAccessCommand
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        RequestText = null
                                    });
    }

    [HttpGet("api/events")]
    public Task<IEnumerable<EventSearchResult>> Search(string searchString,
                                                       bool includeRequestedEvents = false,
                                                       bool includeAuthorizedEvents = false)
    {
        return _mediator.Send(new SearchEventQuery
                              {
                                  SearchString = searchString,
                                  IncludeRequestedEvents = includeRequestedEvents,
                                  IncludeAuthorizedEvents = includeAuthorizedEvents
                              });
    }

    [HttpGet("api/events/{eventAcronym}")]
    public async Task<EventDetails> GetEventDetails(string eventAcronym)
    {
        return await _mediator.Send(new EventQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }
}