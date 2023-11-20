using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;
using EventRegistrar.Backend.Registrations.Price;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfRegistrationSettledAfterPaymentCommandWhenPriceChanged : IEventToCommandTranslation<PriceChanged>
{
    public IEnumerable<IRequest> Translate(PriceChanged e)
    {
        yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId };
    }
}