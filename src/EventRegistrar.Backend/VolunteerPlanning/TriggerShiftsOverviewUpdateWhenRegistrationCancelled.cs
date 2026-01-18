using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class TriggerShiftsOverviewUpdateWhenRegistrationCancelled(IDateTimeProvider dateTimeProvider)
    : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(ShiftsOverviewQuery),
                             EventId = e.EventId.Value,
                             DirtyMoment = dateTimeProvider.Now
                         };
        }
    }
}