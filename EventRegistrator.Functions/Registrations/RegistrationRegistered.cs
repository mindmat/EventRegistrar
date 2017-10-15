using System;

namespace EventRegistrator.Functions.Registrations
{
    public class RegistrationRegistered
    {
        public Guid RegistrationId { get; set; }
        public Registration Registration { get; set; }
    }
}