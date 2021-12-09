namespace EventRegistrar.Backend.Registrations.Overview;

public class CheckinViewItem
{
    public DateTime? AdmittedAt { get; set; }
    public IDictionary<string, string> Columns { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid RegistrationId { get; set; }
    public string Status { get; set; }
    public decimal UnsettledAmount { get; set; }
}