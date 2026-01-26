using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class UnassignFromShiftCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool FromResponsible { get; set; }
}

public class UnassignFromShiftCommandHandler(IRepository<Shift> shifts,
                                             IRepository<ShiftAssignment> assignments,
                                             ChangeTrigger changeTrigger)
    : IRequestHandler<UnassignFromShiftCommand>
{
    public async Task Handle(UnassignFromShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.Where(sft => sft.EventId == command.EventId
                                           && sft.Id == command.ShiftId)
                                .Include(sft => sft.Assignments)
                                .FirstAsync(cancellationToken);

        if (command.FromResponsible)
        {
            // Remove as responsible person
            shift.RegistrationId_Responsible = null;
        }
        else
        {
            // Remove as helper
            var assignment = shift.Assignments!.FirstOrDefault(sas => sas.RegistrationId == command.RegistrationId);
            if (assignment != null)
            {
                assignments.Remove(assignment);
            }
        }


        // Trigger update for the registration that was unassigned
        changeTrigger.TriggerUpdate<RegistrationCalculator>(eventId: command.EventId, rowId: command.RegistrationId);
        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}