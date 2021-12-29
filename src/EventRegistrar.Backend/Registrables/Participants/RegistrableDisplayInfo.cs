namespace EventRegistrar.Backend.Registrables.Participants;

public class RegistrableDisplayInfo
{
    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; }
    public bool HasWaitingList { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public int? MaximumDoubleSeats { get; set; }
    public int? MaximumSingleSeats { get; set; }
    public IEnumerable<SpotDisplayInfo> Participants { get; set; } = null!;
    public IEnumerable<SpotDisplayInfo> WaitingList { get; set; } = null!;
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public int AcceptedLeaders { get; set; }
    public int AcceptedFollowers { get; set; }
}