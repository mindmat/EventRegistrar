using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.Participants;

public class RegistrationDisplayInfo
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public Guid? Id { get; set; }
    public string LastName { get; set; }
    public RegistrationState State { get; set; }
}