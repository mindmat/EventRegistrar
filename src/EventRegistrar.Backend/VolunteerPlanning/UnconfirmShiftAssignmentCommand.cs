using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class UnconfirmShiftAssignmentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftAssignmentId { get; set; }
}

public class UnconfirmShiftAssignmentCommandHandler(IRepository<ShiftAssignment> assignments,
                                                    ChangeTrigger changeTrigger)
    : IRequestHandler<UnconfirmShiftAssignmentCommand>
{
    public async Task Handle(UnconfirmShiftAssignmentCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignments.Where(sas => sas.Shift!.EventId == command.EventId
                                                     && sas.Id == command.ShiftAssignmentId)
                                          .FirstAsync(cancellationToken);

        assignment.IsConfirmed = false;

        // Trigger update for the registration that was unconfirmed
        changeTrigger.TriggerUpdate<RegistrationCalculator>(eventId: command.EventId, rowId: assignment.RegistrationId);

        // Trigger update for the shifts overview
        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}