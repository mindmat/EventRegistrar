using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AccessToEventRequest : Entity
    {
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public string Identifier { get; set; }
        public string IdentityProvider { get; set; }
        public DateTime RequestReceived { get; set; }
        public Guid? UserId { get; set; }
    }
}