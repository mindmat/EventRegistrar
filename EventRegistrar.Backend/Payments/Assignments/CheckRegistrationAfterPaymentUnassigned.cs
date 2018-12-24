using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class CheckRegistrationAfterPaymentUnassigned : IEventToCommandTranslation<PaymentUnassigned>
    {
        public IEnumerable<IRequest> Translate(PaymentUnassigned e)
        {
            if (e.RegistrationId != null)
            {
                yield return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId.Value };
            }
        }
    }
}