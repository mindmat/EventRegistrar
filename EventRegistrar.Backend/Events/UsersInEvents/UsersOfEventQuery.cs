using MediatR;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UsersOfEventQuery : IRequest<IEnumerable<UserInEventDisplayItem>>
    {
        public string EventAcronym { get; set; }
    }
}