using System;
using EventRegistrar.Backend.Registrables;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class SetUserRoleInEventCommand : IRequest<Unit>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public UserInEventRole Role { get; set; }
        public Guid UserId { get; set; }
    }
}