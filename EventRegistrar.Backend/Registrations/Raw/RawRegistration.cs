using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrations.Raw
{
    public class RawRegistration : Entity
    {
        public DateTime Created { get; set; }
        public Guid EventId { get; set; }
        public string FormExternalIdentifier { get; set; }
        public DateTime? Processed { get; set; }
        public string ReceivedMessage { get; set; }
        public string RegistrationExternalIdentifier { get; set; }
    }
}