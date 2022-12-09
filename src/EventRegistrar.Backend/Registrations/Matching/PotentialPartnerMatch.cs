namespace EventRegistrar.Backend.Registrations.Matching;

public class PotentialPartnerMatch
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsOnWaitingList { get; set; }

    public string? DeclaredPartner { get; set; }
    public string? MatchedPartner { get; set; }
    public Guid? RegistrationId_Partner { get; set; }
    public IEnumerable<TrackMatch>? Tracks { get; set; }
    public string State { get; set; }
}

public class TrackMatch
{
    public string? Name { get; set; }
    public TracksMatch Match { get; set; }
}

public enum TracksMatch
{
    None = 1,
    Some = 2,
    All = 3
}