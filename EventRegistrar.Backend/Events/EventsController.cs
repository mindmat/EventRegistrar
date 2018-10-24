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

        [HttpPut("api/events/{eventAcronym}")]
        public Task CreateEvent(string eventAcronym, string name, Guid id, Guid? eventId_CopyFrom)
        {
            return _mediator.Send(new CreateEventCommand
            {
                Acronym = eventAcronym,
                Id = id,
                EventId_CopyFrom = eventId_CopyFrom,
                Name = name
            });
        }

        [HttpGet("api/events/{eventAcronym}/requests")]
        public Task<IEnumerable<AccessRequestOfEvent>> GetRequestsOfEvent(string eventAcronym, bool includeDeniedRequests = false)
        {
            return _mediator.Send(new AccessRequestsOfEventQuery
            {
                EventAcronym = eventAcronym,
                IncludeDeniedRequests = includeDeniedRequests
            });
        }

        [HttpGet("api/events/{eventAcronym}/users")]
        public Task<IEnumerable<UserInEventDisplayItem>> GetUsersOfEvent(string eventAcronym)
        {
            return _mediator.Send(new UsersOfEventQuery
            {
                EventAcronym = eventAcronym
            });
        }

        [HttpPost("api/events/{eventAcronym}/requestAccess")]
        public Task<Guid> RequestAccess(string eventAcronym)
        {
            return _mediator.Send(new RequestAccessCommand
            {
                EventAcronym = eventAcronym,
                RequestText = null
            });
        }

        [HttpPost("api/events/{eventAcronym}/accessrequests/{accessRequestId:guid}/respond")]
        public Task RespondToAccessRequest(string eventAcronym,
                                           Guid accessRequestId,
                                           [FromBody]RequestResponseDto response)
        {
            return _mediator.Send(new RespondToRequestCommand
            {
                EventAcronym = eventAcronym,
                AccessToEventRequestId = accessRequestId,
                Response = response.Response,
                Role = response.Role,
                ResponseText = response.Text
            });
        }

        [HttpGet("api/events")]
        public Task<IEnumerable<EventSearchResult>> Search(string searchString, bool includeRequestedEvents = false, bool includeAuthorizedEvents = false)
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