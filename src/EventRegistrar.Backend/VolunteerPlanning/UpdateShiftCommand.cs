using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class UpdateShiftCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ShiftId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public Guid? RegistrableId_ShiftPreference { get; set; }
    public Guid? RegistrationId_Responsible { get; set; }
}

public class UpdateShiftCommandHandler(IRepository<Shift> shifts, ChangeTrigger changeTrigger)
    : IRequestHandler<UpdateShiftCommand>
{
    public async Task Handle(UpdateShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.FirstAsync(sft => sft.Id == command.ShiftId
                                                && sft.EventId == command.EventId, cancellationToken);

        shift.Name = command.Name;
        shift.Description = command.Description;
        shift.Location = command.Location;
        shift.StartTime = command.StartTime;
        shift.EndTime = command.EndTime;
        shift.RegistrableId_ShiftPreference = command.RegistrableId_ShiftPreference;
        shift.RegistrationId_Responsible = command.RegistrationId_Responsible;

        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}