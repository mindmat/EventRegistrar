using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class TryMoveUpFromWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
{
    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.SpotWasOnWaitingList && e.Reason == RemoveSpotReason.Modification)
        {
            yield return new TriggerMoveUpFromWaitingListCommand { RegistrableId = e.RegistrableId };
        }
    }
}