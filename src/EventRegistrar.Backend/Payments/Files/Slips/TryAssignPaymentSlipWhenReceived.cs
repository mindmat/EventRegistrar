using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class TryAssignPaymentSlipWhenReceived : IEventToCommandTranslation<PaymentSlipReceived>
{
    public IEnumerable<IRequest> Translate(PaymentSlipReceived e)
    {
        if (e.EventId.HasValue)
            yield return new TryAssignPaymentSlipCommand
                         {
                             PaymentSlipId = e.PaymentSlipId,
                             Reference = e.Reference,
                             EventId = e.EventId.Value
                         };
    }
}