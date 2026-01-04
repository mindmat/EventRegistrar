using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class AssignToShiftCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Notes { get; set; }
    public bool AsResponsible { get; set; }
}

public class AssignToShiftCommandHandler(IRepository<Shift> shifts,
                                         IRepository<ShiftAssignment> assignments,
                                         ChangeTrigger changeTrigger)
    : IRequestHandler<AssignToShiftCommand>
{
    public async Task Handle(AssignToShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.Where(sft => sft.EventId == command.EventId
                                           && sft.Id == command.ShiftId)
                                .Include(sft => sft.Assignments)
                                .FirstAsync(cancellationToken);

        if (command.AsResponsible)
        {
            // Assign as responsible person
            shift.RegistrationId_Responsible = command.RegistrationId;
        }
        else
        {
            if (shift.Assignments!.Any(sas => sas.RegistrationId == command.RegistrationId))
            {
                // Registration is already assigned to this shift
                return;
            }

            assignments.InsertObjectTree(new ShiftAssignment
                                         {
                                             Id = Guid.NewGuid(),
                                             ShiftId = command.ShiftId,
                                             RegistrationId = command.RegistrationId
                                         });
        }

        changeTrigger.QueryChanged<ShiftsOverviewQuery>(command.EventId);
    }
}