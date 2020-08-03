using System;

using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Registrations.Register
{
    public interface IRegistrationProcessConfiguration
    {
        Guid Id { get; }
        Guid RegistrationFormId { get; }
        string? Description { get; }
        public FormPathType Type { get; }
    }
}