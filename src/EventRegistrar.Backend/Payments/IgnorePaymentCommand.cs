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

internal class IgnorePaymentCommandHandler : AsyncRequestHandler<IgnorePaymentCommand>
{
    private readonly IRepository<Payment> _payments;
    private readonly IEventBus _eventBus;

    public IgnorePaymentCommandHandler(IRepository<Payment> payments,
                                       IEventBus eventBus)
    {
        _payments = payments;
        _eventBus = eventBus;
    }

    protected override async Task Handle(IgnorePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await _payments.AsTracking()
                                     .FirstAsync(pmt => pmt.Id == command.PaymentId
                                                     && pmt.PaymentsFile!.EventId == command.EventId,
                                                 cancellationToken);
        payment.Ignore = true;

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(BookingsByStateQuery)
                          });
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(PaymentsByDayQuery)
                          });
    }
}