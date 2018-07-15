using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEventsController : Controller
    {
        private readonly IMediator _mediator;

        public UserInEventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("api/events/{eventAcronym}/users/{userId:guid}/roles/{role}")]
        public Task AddRole(string eventAcronym, Guid userId, UserInEventRole role)
        {
            return _mediator.Send(new AddUserToRoleInEventCommand
            {
                EventAcronym = eventAcronym,
                UserId = userId,
                Role = role
            });
        }

        [HttpGet("api/me/events")]
        public Task<IEnumerable<UserInEventDisplayItem>> GetMyEvents(bool includeRequestedEvents)
        {
            return _mediator.Send(new EventsOfUserQuery { IncludeRequestedEvents = includeRequestedEvents });
        }

        [HttpDelete("api/events/{eventAcronym}/users/{userId:guid}/roles/{role}")]
        public Task RemoveRole(string eventAcronym, Guid userId, UserInEventRole role)
        {
            return _mediator.Send(new RemoveUserFromRoleInEventCommand
            {
                EventAcronym = eventAcronym,
                UserId = userId,
                Role = role
            });
        }
    }
}