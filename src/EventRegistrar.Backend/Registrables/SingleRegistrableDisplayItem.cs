namespace EventRegistrar.Backend.Registrables;

public class SingleRegistrableDisplayItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Accepted { get; set; }
    public int? OnWaitingList { get; set; }
    public int? SpotsAvailable { get; set; }
    public bool HasWaitingList { get; set; }
    public bool IsDeletable { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public IEnumerable<SpotState> Class { get; set; } = null!;
    public IEnumerable<SpotState> WaitingList { get; set; } = null!;
}