using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignRepaymentCommand : IRequest, IEventBoundRequest
{
    public decimal Amount { get; set; }
    public Guid EventId { get; set; }
    public Guid IncomingPaymentId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
}

public class AssignRepaymentCommandHandler(IQueryable<IncomingPayment> incomingPayments,
                                           IQueryable<OutgoingPayment> outgoingPayments,
                                           IRepository<PaymentAssignment> assignments,
                                           IEventBus eventBus,
                                           IDateTimeProvider dateTimeProvider)
    : IRequestHandler<AssignRepaymentCommand>
{
    public async Task Handle(AssignRepaymentCommand command, CancellationToken cancellationToken)
    {
        var incomingPayment = await incomingPayments.Where(pmt => pmt.Id == command.IncomingPaymentId
                                                               && pmt.Payment!.EventId == command.EventId)
                                                    .Include(pmt => pmt.Payment)
                                                    .FirstAsync(cancellationToken);
        var outgoingPayment = await outgoingPayments.Where(pmt => pmt.Id == command.OutgoingPaymentId
                                                               && pmt.Payment!.EventId == command.EventId)
                                                    .Include(pmt => pmt.Payment)
                                                    .FirstAsync(cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             IncomingPaymentId = incomingPayment.Id,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = command.Amount,
                             Created = dateTimeProvider.Now
                         };
        assignments.InsertObjectTree(assignment);

        eventBus.Publish(new RepaymentAssigned
                         {
                             PaymentAssignmentId = assignment.Id,
                             Amount = assignment.Amount,
                             EventId = command.EventId,
                             IncomingPaymentId = incomingPayment.Id,
                             OutgoingPaymentId = outgoingPayment.Id
                         });
    }
}