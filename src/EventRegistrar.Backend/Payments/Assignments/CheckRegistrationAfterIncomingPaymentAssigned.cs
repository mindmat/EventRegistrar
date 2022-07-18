using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

using MediatR;

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