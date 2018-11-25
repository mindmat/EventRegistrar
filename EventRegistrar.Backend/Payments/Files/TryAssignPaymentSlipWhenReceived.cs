using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Payments.Files
{
    public class TryAssignPaymentSlipWhenReceived : IEventToCommandTranslation<PaymentSlipReceived>
    {
        public IEnumerable<IQueueBoundMessage> Translate(PaymentSlipReceived e)
        {
            if (e.EventId.HasValue)
            {
                yield return new TryAssignPaymentSlipCommand
                {
                    PaymentSlipId = e.PaymentSlipId,
                    Reference = e.Reference,
                    EventId = e.EventId.Value
                };
            }
        }
    }
}