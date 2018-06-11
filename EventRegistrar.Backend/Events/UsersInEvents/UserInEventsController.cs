using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEventsController : Controller
    {
        private readonly IMediator _mediator;

        public UserInEventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me/events")]
        public Task<IEnumerable<UserInEventDisplayItem>> GetMyEvents()
        {
            return _mediator.Send(new EventsOfUserQuery());
        }
    }
}