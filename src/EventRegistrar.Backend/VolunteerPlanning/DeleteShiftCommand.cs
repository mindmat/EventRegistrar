using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations;

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
        var shift = await shifts.Where(sft => sft.Id == command.ShiftId
                                           && sft.EventId == command.EventId)
                                .Include(sft => sft.Assignments)
                                .FirstAsync(cancellationToken);

        // Trigger updates for all affected registrations
        foreach (var registrationId in shift.Assignments!.Select(sas => sas.RegistrationId)
                                            .AppendIfNotNull(shift.RegistrationId_Responsible))
        {
            changeTrigger.TriggerUpdate<RegistrationCalculator>(command.EventId, registrationId);
        }

        shifts.Remove(shift);

        changeTrigger.TriggerUpdate<ShiftsOverviewCalculator>(eventId: command.EventId);
    }
}