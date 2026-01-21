using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class ConfirmShiftAssignmentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftAssignmentId { get; set; }
}

public class ConfirmShiftAssignmentCommandHandler(IRepository<ShiftAssignment> assignments,
                                                  ChangeTrigger changeTrigger)
    : IRequestHandler<ConfirmShiftAssignmentCommand>
{
    public async Task Handle(ConfirmShiftAssignmentCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignments.Where(sas => sas.Shift!.EventId == command.EventId
                                                     && sas.Id == command.ShiftAssignmentId)
                                          .FirstAsync(cancellationToken);

        assignment.IsConfirmed = true;

        // Trigger update for the registration that was confirmed
        changeTrigger.TriggerUpdate<RegistrationCalculator>(command.EventId, assignment.RegistrationId);
        
        // Trigger update for the shifts overview
        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(command.EventId);
    }
}