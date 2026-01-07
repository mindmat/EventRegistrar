namespace EventRegistrar.Backend.VolunteerPlanning;

public class ShiftsOverviewQuery : IRequest<IEnumerable<ShiftGroup>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ShiftsOverviewQueryHandler(IQueryable<Shift> shifts)
    : IRequestHandler<ShiftsOverviewQuery, IEnumerable<ShiftGroup>>
{
    private static readonly TimeSpan GroupByDaySkew = new(5, 0, 0);

    public async Task<IEnumerable<ShiftGroup>> Handle(ShiftsOverviewQuery query, CancellationToken cancellationToken)
    {
        var data = await shifts.Where(shift => shift.EventId == query.EventId)
                               .Select(shift => new ShiftDisplayItem
                                                {
                                                    Id = shift.Id,
                                                    Name = shift.Name,
                                                    Description = shift.Description,
                                                    Location = shift.Location,
                                                    StartTime = shift.StartTime,
                                                    EndTime = shift.EndTime,
                                                    HelpersNeeded = shift.HelpersNeeded,
                                                    HelpersAssigned = shift.Assignments!.Count,
                                                    ResponsibleRegistrationId = shift.RegistrationId_Responsible,
                                                    ParticipantResponsible = $"{shift.Registration_Responsible!.RespondentFirstName} {shift.Registration_Responsible!.RespondentLastName}",
                                                    ResponsibleEmail = shift.Registration_Responsible!.RespondentEmail,
                                                    ShiftPreferenceRegistrableId = shift.RegistrableId_ShiftPreference,
                                                    ShiftPreferenceRegistrableName = shift.Registrable_ShiftPreference!.Name,
                                                    ShiftPreferenceRegistrableNameSecondary = shift.Registrable_ShiftPreference!.NameSecondary,
                                                    Assignments = shift.Assignments!.Select(a => new ShiftAssignmentDisplayItem
                                                                                                 {
                                                                                                     Id = a.Id,
                                                                                                     RegistrationId = a.RegistrationId,
                                                                                                     Participant = $"{a.Registration!.RespondentFirstName} {a.Registration!.RespondentLastName}",
                                                                                                     Email = a.Registration!.RespondentEmail,
                                                                                                 })
                                                                       .ToList()
                                                })
                               .OrderBy(sft => sft.StartTime)
                               .ToListAsync(cancellationToken);
        return data.GroupBy(sft => new
                                   {
                                       Day = (sft.StartTime - GroupByDaySkew).Date,
                                       sft.Location
                                   })
                   .Select(grp => new ShiftGroup(grp.Key.Day, grp.Key.Location, grp.ToList()));
    }
}

public record ShiftGroup(DateTime Day, string? Location, IEnumerable<ShiftDisplayItem> Shifts);

public class ShiftDisplayItem
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int HelpersNeeded { get; set; }
    public int HelpersAssigned { get; set; }

    // Responsible person info
    public Guid? ResponsibleRegistrationId { get; set; }
    public string? ParticipantResponsible { get; set; }
    public string? ResponsibleEmail { get; set; }

    // Shift preference info
    public Guid? ShiftPreferenceRegistrableId { get; set; }
    public string? ShiftPreferenceRegistrableName { get; set; }
    public string? ShiftPreferenceRegistrableNameSecondary { get; set; }

    // Assignment info
    public ICollection<ShiftAssignmentDisplayItem> Assignments { get; set; } = [];
}

public class ShiftAssignmentDisplayItem
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Participant { get; set; }
    public string? Email { get; set; }
}