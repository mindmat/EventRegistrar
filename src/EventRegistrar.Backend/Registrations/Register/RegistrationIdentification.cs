namespace EventRegistrar.Backend.Registrations.Register;

public class RegistrationIdentification(Registration registration)
{
    public Guid? Id { get; } = registration.Id;
    public string? Email { get; } = registration.RespondentEmail?.ToLowerInvariant();
    public string? FirstName { get; } = registration.RespondentFirstName?.ToLowerInvariant();
    public string? LastName { get; } = registration.RespondentLastName?.ToLowerInvariant();
}