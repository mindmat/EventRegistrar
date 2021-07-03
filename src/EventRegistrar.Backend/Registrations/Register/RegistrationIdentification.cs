using System;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class RegistrationIdentification
    {
        public RegistrationIdentification(Registration registration)
        {
            Id = registration.Id;
            Email = registration.RespondentEmail?.ToLowerInvariant();
            FirstName = registration.RespondentFirstName?.ToLowerInvariant();
            LastName = registration.RespondentLastName?.ToLowerInvariant();
        }

        public Guid? Id { get; }
        public string? Email { get; }
        public string? FirstName { get; }
        public string? LastName { get; }
    }
}