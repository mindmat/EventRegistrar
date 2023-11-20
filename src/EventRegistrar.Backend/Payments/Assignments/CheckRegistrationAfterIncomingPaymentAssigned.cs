using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CheckRegistrationAfterIncomingPaymentAssigned : IEventToCommandTranslation<IncomingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.RegistrationId != null)
        {
            yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId.Value };
        }
    }
}