using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class SetPaidIfPayAtCheckinIsSet : IEventToCommandTranslation<WillPayAtCheckinSet>
{
    public IEnumerable<IRequest> Translate(WillPayAtCheckinSet e)
    {
        yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId };
    }
}