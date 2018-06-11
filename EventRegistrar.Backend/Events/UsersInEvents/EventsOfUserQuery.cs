using MediatR;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class EventsOfUserQuery : IRequest<IEnumerable<UserInEventDisplayItem>>
    {
    }
}