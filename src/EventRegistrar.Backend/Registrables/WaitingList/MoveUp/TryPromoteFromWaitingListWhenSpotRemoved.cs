using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class TryPromoteFromWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
{
    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.WasSpotOnWaitingList && e.Reason == RemoveSpotReason.Modification)
        {
            yield return new TriggerMoveUpFromWaitingListCommand { RegistrableId = e.RegistrableId };
        }
    }
}