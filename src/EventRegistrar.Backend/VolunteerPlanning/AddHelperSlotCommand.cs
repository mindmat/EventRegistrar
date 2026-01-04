using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class AddHelperSlotCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
}

public class AddHelperSlotCommandHandler(IRepository<Shift> shifts,
                                         ChangeTrigger changeTrigger)
    : IRequestHandler<AddHelperSlotCommand>
{
    public async Task Handle(AddHelperSlotCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.Where(sft => sft.EventId == command.EventId
                                           && sft.Id == command.ShiftId)
                                .FirstAsync(cancellationToken);

        shift.HelpersNeeded++;
        changeTrigger.QueryChanged<ShiftsOverviewQuery>(command.EventId);
    }
}