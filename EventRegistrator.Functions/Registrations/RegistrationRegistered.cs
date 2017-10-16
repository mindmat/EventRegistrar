using System;

namespace EventRegistrator.Functions.Registrations
{
    public class RegistrationRegistered
    {
        public Guid? EventId { get; set; }
        public Guid RegistrationId { get; set; }
        public Registration Registration { get; set; }
    }
}