using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CheckRegistrationAfterPaymentAssigned : IEventToCommandTranslation<PaymentAssigned>
{
    public IEnumerable<IRequest> Translate(PaymentAssigned e)
    {
        if (e.RegistrationId != null)
            yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId.Value };
    }
}