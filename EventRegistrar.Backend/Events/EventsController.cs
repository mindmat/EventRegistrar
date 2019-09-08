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
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator,
                                IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
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
        public async Task<IEnumerable<AccessRequestOfEvent>> GetRequestsOfEvent(string eventAcronym, bool includeDeniedRequests = false)
        {
            return await _mediator.Send(new AccessRequestsOfEventQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                IncludeDeniedRequests = includeDeniedRequests
            });
        }

        [HttpGet("api/events/{eventAcronym}/users")]
        public async Task<IEnumerable<UserInEventDisplayItem>> GetUsersOfEvent(string eventAcronym)
        {
            return await _mediator.Send(new UsersOfEventQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpPost("api/events/{eventAcronym}/openRegistration")]
        public async Task OpenRegistration(string eventAcronym)
        {
            await _mediator.Send(new OpenRegistrationCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                DeleteData = true
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

        [HttpPost("api/events/{eventAcronym}/accessrequests/{accessRequestId:guid}/respond")]
        public async Task RespondToAccessRequest(string eventAcronym,
                                                 Guid accessRequestId,
                                                 [FromBody]RequestResponseDto response)
        {
            await _mediator.Send(new RespondToRequestCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
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

        [HttpGet("api/events/{eventAcronym}")]
        public async Task<EventDetails> GetEventDetails(string eventAcronym)
        {
            return await _mediator.Send(new EventQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }
    }
}