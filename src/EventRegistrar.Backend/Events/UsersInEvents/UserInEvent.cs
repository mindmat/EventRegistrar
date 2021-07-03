using EventRegistrar.Backend.Infrastructure.DataAccess;
using System;
using EventRegistrar.Backend.Authentication.Users;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEvent : Entity
    {
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public UserInEventRole Role { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}