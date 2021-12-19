namespace EventRegistrar.Backend.Registrables;

public record DoubleRegistrableDisplayItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; }
    public int CouplesOnWaitingList { get; set; }
    public int FollowersAccepted { get; set; }
    public int FollowersOnWaitingList { get; set; }
    public int LeadersAccepted { get; set; }
    public int LeadersOnWaitingList { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public int? SpotsAvailable { get; set; }
    public bool IsDeletable { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public IEnumerable<DoubleSpotState> Class { get; set; } = null!;
    public IEnumerable<DoubleSpotState> WaitingList { get; set; } = null!;
}