namespace EventRegistrar.Backend.Registrations.Search;

public class RegistrationMatch
{
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public bool IsWaitingList { get; set; }
    public string LastName { get; set; }
    public Guid RegistrationId { get; set; }
    public RegistrationState State { get; set; }
    public string StateText { get; set; }
    public IEnumerable<string> Registrables { get; set; }
}