using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.Participants;

public class RegistrationDisplayInfo
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public RegistrationState State { get; set; }
}