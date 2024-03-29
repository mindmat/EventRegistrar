﻿using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.Spots;

public class SpotDisplayItem
{
    public DateTimeOffset FirstPartnerJoined { get; set; }
    public Guid Id { get; set; }
    public bool IsCore { get; set; }
    public bool IsWaitingList { get; set; }
    public string? PartnerName { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public string RegistrableName { get; set; } = null!;
    public string? RegistrableNameSecondary { get; set; }
    public Guid RegistrableId { get; set; }
    public RegistrableType Type { get; set; }
    public string? RoleText { get; set; }
}