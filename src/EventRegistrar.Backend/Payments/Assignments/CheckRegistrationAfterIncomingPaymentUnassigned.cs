using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CheckRegistrationAfterIncomingPaymentUnassigned : IEventToCommandTranslation<IncomingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.RegistrationId != null)
        {
            yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId.Value };
        }
    }
}