using EventRegistrator.Functions.Users;
using System;

namespace EventRegistrator.Functions.Events
{
    public class UserInEvent
    {
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public UserInEventRole Role { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}