namespace EventRegistrar.Backend.Hosting;

public class HostingOffer
{
    public DateTimeOffset? AdmittedAt { get; set; }
    public Dictionary<string, string> Columns { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Language { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public Guid RegistrationId { get; set; }
    public string State { get; set; }
}

public class HostingOffers
{
    public IEnumerable<string> DynamicColumns;
    public IEnumerable<HostingOffer> Offers;
}