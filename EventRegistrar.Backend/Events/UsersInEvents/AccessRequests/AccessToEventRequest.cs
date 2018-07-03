using System;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class AccessToEventRequest : Entity
    {
        public string Email { get; set; }
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public string FirstName { get; set; }
        public string Identifier { get; set; }
        public IdentityProvider IdentityProvider { get; set; }
        public string LastName { get; set; }
        public DateTime RequestReceived { get; set; }
        public string RequestText { get; set; }
        public RequestResponse? Response { get; set; }
        public string ResponseText { get; set; }
        public Guid? UserId_Requestor { get; set; }
        public Guid? UserId_Responder { get; set; }
    }
}