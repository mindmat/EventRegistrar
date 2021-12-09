namespace EventRegistrar.Backend.Registrables.Participants;

public class RegistrableDisplayInfo
{
    public bool HasWaitingList { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public int? MaximumDoubleSeats { get; set; }
    public int? MaximumSingleSeats { get; set; }
    public string Name { get; set; }
    public IEnumerable<PlaceDisplayInfo> Participants { get; set; }
    public IEnumerable<PlaceDisplayInfo> WaitingList { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
}