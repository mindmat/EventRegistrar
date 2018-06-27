using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events
{
    public class EventsController : Controller
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/events/{eventAcronym}/requestAccess")]
        public Task RequestAccess(string eventAcronym)
        {
            return _mediator.Send(new RequestAccessCommand { EventAcronym = eventAcronym });
        }

        [HttpGet("api/events")]
        public Task<IEnumerable<EventSearchResult>> Search(string searchString, bool includeRequestedEvents, bool includeAuthorizedEvents)
        {
            return _mediator.Send(new SearchEventQuery
            {
                SearchString = searchString,
                IncludeRequestedEvents = includeRequestedEvents,
                IncludeAuthorizedEvents = includeAuthorizedEvents
            });
        }
    }
}