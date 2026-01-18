using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class RemoveHelperSlotCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
}

public class RemoveHelperSlotCommandHandler(IRepository<Shift> shifts,
                                            ChangeTrigger changeTrigger)
    : IRequestHandler<RemoveHelperSlotCommand>
{
    public async Task Handle(RemoveHelperSlotCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.Where(sft => sft.EventId == command.EventId
                                           && sft.Id == command.ShiftId)
                                .Include(sft => sft.Assignments)
                                .FirstAsync(cancellationToken);

        var currentAssignmentsCount = shift.Assignments!.Count;

        if (shift.HelpersNeeded < 0)
        {
            throw new InvalidOperationException("Cannot reduce helper slots below zero");
        }

        var emptySlots = shift.HelpersNeeded - currentAssignmentsCount;
        if (emptySlots < 1)
        {
            throw new InvalidOperationException("Cannot remove assigned helper slots. Please unassign participants first.");
        }

        shift.HelpersNeeded--;

        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}