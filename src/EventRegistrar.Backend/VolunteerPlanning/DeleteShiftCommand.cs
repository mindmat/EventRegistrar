using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class DeleteShiftCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
}

public class DeleteShiftCommandHandler(IRepository<Shift> shifts, ChangeTrigger changeTrigger)
    : IRequestHandler<DeleteShiftCommand>
{
    public async Task Handle(DeleteShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.FirstAsync(sft => sft.Id == command.ShiftId
                                                && sft.EventId == command.EventId,
                                            cancellationToken);

        shifts.Remove(shift);
        changeTrigger.QueryChanged<ShiftsOverviewQuery>(command.EventId);
    }
}