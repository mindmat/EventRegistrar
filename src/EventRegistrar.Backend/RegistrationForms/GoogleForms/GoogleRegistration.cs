namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class GoogleRegistration
{
    public string? Email { get; set; }
    public IEnumerable<ResponseData>? Responses { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}