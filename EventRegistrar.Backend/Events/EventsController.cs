using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
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
        public Task RequestAccess(string eventAcronym, [FromBody]string text)
        {
            return _mediator.Send(new RequestAccessCommand
            {
                EventAcronym = eventAcronym,
                RequestText = text
            });
        }

        [HttpPost("api/accessrequest/{accessRequestId:guid}/respond")]
        public Task RespondToAccessRequest(Guid accessRequestId,
                                           string text,
                                           RequestResponse response,
                                           UserInEventRole role)
        {
            return _mediator.Send(new RespondToRequestCommand
            {
                AccessToEventRequestId = accessRequestId,
                Response = response,
                Role = role,
                ResponseText = text
            });
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