namespace EventRegistrar.Backend.Hosting;

public class HostingRequest
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

public class HostingRequests
{
    public IEnumerable<string> DynamicColumns;
    public IEnumerable<HostingRequest> Requests;
}