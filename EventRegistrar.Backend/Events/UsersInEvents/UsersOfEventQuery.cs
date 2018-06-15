using System.Collections.Generic;
using EventRegistrar.Backend.Registrables;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UsersOfEventQuery : IRequest<IEnumerable<UserInEventDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}