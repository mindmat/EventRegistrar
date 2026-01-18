using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class CreateShiftCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

public class CreateShiftCommandHandler(IRepository<Shift> shifts, ChangeTrigger changeTrigger)
    : IRequestHandler<CreateShiftCommand>
{
    public async Task Handle(CreateShiftCommand command, CancellationToken cancellationToken)
    {
        await shifts.InsertOrUpdateEntity(new Shift
                                          {
                                              Id = command.ShiftId,
                                              EventId = command.EventId,
                                              Location = command.Location,
                                              StartTime = command.StartTime,
                                              EndTime = command.EndTime,
                                              HelpersNeeded = 1
                                          }, cancellationToken);

        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}