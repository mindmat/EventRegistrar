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

        [HttpGet("api/me/events")]
        public Task<IEnumerable<UserInEventDisplayItem>> GetMyEvents(bool includeRequestedEvents)
        {
            return _mediator.Send(new EventsOfUserQuery { IncludeRequestedEvents = includeRequestedEvents });
        }
    }
}