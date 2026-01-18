using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Infrastructure.MenuNodes;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class ShiftsOverviewQuery : IRequest<SerializedJson<IEnumerable<ShiftGroup>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ShiftsOverviewQueryHandler(ReadModelReader readModelReader)
    : IRequestHandler<ShiftsOverviewQuery, SerializedJson<IEnumerable<ShiftGroup>>>
{
    public async Task<SerializedJson<IEnumerable<ShiftGroup>>> Handle(ShiftsOverviewQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<ShiftGroup>>(nameof(ShiftsOverviewQuery),
                                                                  query.EventId,
                                                                  null,
                                                                  cancellationToken);
    }
}

public class ShiftsOverviewCalculator(IQueryable<Shift> shifts)
    : ReadModelCalculator<IEnumerable<ShiftGroup>>
{
    private static readonly TimeSpan GroupByDaySkew = new(5, 0, 0);

    public override string QueryName => nameof(ShiftsOverviewQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(IEnumerable<ShiftGroup> ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var data = await shifts.Where(shift => shift.EventId == eventId)
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
                                                    IsParticipantResponsibleCancelled = shift.Registration_Responsible!.State == RegistrationState.Cancelled,
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
                                                                                                     IsRegistrationCancelled = a.Registration!.State == RegistrationState.Cancelled
                                                                                                 })
                                                                       .ToList()
                                                })
                               .OrderBy(sft => sft.StartTime)
                               .ToListAsync(cancellationToken);

        var shiftGroups = data.GroupBy(sft => new
                                              {
                                                  Day = (sft.StartTime - GroupByDaySkew).Date,
                                                  sft.Location
                                              })
                              .Select(grp => new ShiftGroup(grp.Key.Day, grp.Key.Location, grp.ToList()))
                              .ToList();

        var node = CalculateNode(shiftGroups);

        return (shiftGroups, node);
    }

    private static MenuNodeCalculation CalculateNode(List<ShiftGroup> shiftGroups)
    {
        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.ShiftsOverview
                   };

        var allShifts = shiftGroups.SelectMany(group => group.Shifts).ToList();
        var unassignedShifts = allShifts.Sum(shift => (shift.ResponsibleRegistrationId == null ? 1 : 0)
                                                    + Math.Max(shift.HelpersNeeded - shift.HelpersAssigned, 0));
        var shiftsWithCancelledRegistrations = allShifts.Sum(shift => (shift.IsParticipantResponsibleCancelled ? 1 : 0)
                                                                    + shift.Assignments.Count(assignment => assignment.IsRegistrationCancelled));

        if (unassignedShifts > 0 || shiftsWithCancelledRegistrations > 0)
        {
            node.Content = $"{unassignedShifts} | {shiftsWithCancelledRegistrations}";
            node.Style = shiftsWithCancelledRegistrations > 0
                             ? MenuNodeStyle.ToDo
                             : MenuNodeStyle.Info;
        }

        return node;
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
    public bool IsParticipantResponsibleCancelled { get; set; }

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
    public bool IsRegistrationCancelled { get; set; }
}