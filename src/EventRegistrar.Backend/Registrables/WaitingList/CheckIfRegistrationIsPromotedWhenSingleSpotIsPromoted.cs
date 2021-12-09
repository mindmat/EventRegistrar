using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class
    CheckIfRegistrationIsPromotedWhenSingleSpotIsPromoted : IEventToCommandTranslation<
        SingleSpotPromotedFromWaitingList>
{
    public IEnumerable<IRequest> Translate(SingleSpotPromotedFromWaitingList e)
    {
        yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
    }
}