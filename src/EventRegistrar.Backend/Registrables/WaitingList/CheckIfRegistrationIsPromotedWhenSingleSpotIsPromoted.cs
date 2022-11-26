using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationIsPromotedWhenSingleSpotIsPromoted : IEventToCommandTranslation<SingleSpotPromotedFromWaitingList>
{
    public IEnumerable<IRequest> Translate(SingleSpotPromotedFromWaitingList e)
    {
        yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
    }
}