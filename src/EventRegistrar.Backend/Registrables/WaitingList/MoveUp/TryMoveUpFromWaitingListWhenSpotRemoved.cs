using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class TryMoveUpFromWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
{
    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e is { SpotWasOnWaitingList: true, Reason: RemoveSpotReason.Modification, EventId: not null })
        {
            yield return new TriggerMoveUpFromWaitingListCommand { EventId = e.EventId.Value, RegistrableId = e.RegistrableId };
        }
    }
}