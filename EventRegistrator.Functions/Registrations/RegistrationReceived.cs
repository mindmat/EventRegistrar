using System;

namespace EventRegistrator.Functions.Registrations
{
    public class RegistrationReceived
    {
        public Guid RegistrationId { get; set; }
        public Registration Registration { get; set; }
    }
}