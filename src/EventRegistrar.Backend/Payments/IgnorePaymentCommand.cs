using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Settlements;
using EventRegistrar.Backend.Payments.Statements;

namespace EventRegistrar.Backend.Payments;

internal class IgnorePaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

internal class IgnorePaymentCommandHandler(IRepository<Payment> payments,
                                           IEventBus eventBus) : IRequestHandler<IgnorePaymentCommand>
{
    public async Task Handle(IgnorePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await payments.AsTracking()
                                    .FirstAsync(pmt => pmt.Id == command.PaymentId
                                                    && pmt.EventId == command.EventId,
                                                cancellationToken);
        payment.Ignore = true;

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(BookingsByStateQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PaymentsByDayQuery)
                         });
    }
}