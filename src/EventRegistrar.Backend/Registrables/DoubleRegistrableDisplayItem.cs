﻿namespace EventRegistrar.Backend.Registrables;

public record DoubleRegistrableDisplayItem
{
    public int CouplesOnWaitingList { get; set; }
    public int FollowersAccepted { get; set; }
    public int FollowersOnWaitingList { get; set; }
    public Guid Id { get; set; }
    public int LeadersAccepted { get; set; }
    public int LeadersOnWaitingList { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; }
    public int? SpotsAvailable { get; set; }
    public bool IsDeletable { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
}