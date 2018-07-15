using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AddUserToRoleInEventCommand : IRequest<Guid>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public UserInEventRole Role { get; set; }
        public Guid UserId { get; set; }
    }
}