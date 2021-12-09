using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignRepaymentCommand : IRequest, IEventBoundRequest
{
    public decimal Amount { get; set; }
    public Guid EventId { get; set; }
    public Guid PaymentId_Incoming { get; set; }
    public Guid PaymentId_Outgoing { get; set; }
}

public class AssignRepaymentCommandHandler : IRequestHandler<AssignRepaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IQueryable<ReceivedPayment> _payments;

    public AssignRepaymentCommandHandler(IQueryable<ReceivedPayment> payments,
                                         IRepository<PaymentAssignment> assignments,
                                         IEventBus eventBus)
    {
        _payments = payments;
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(AssignRepaymentCommand command, CancellationToken cancellationToken)
    {
        var paymentIncoming = await _payments.FirstAsync(pmt => pmt.Id == command.PaymentId_Incoming
                                                             && pmt.PaymentFile.EventId == command.EventId,
            cancellationToken);
        var paymentOutgoing = await _payments.FirstAsync(pmt => pmt.Id == command.PaymentId_Outgoing
                                                             && pmt.PaymentFile.EventId == command.EventId,
            cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             ReceivedPaymentId = paymentIncoming.Id,
                             PaymentId_Repayment = paymentOutgoing.Id,
                             Amount = command.Amount,
                             Created = DateTime.UtcNow
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        _eventBus.Publish(new PaymentAssigned
                          {
                              PaymentAssignmentId = assignment.Id,
                              Amount = assignment.Amount,
                              EventId = command.EventId,
                              PaymentId = paymentIncoming.Id,
                              PaymentId_Counter = paymentOutgoing.Id
                          });

        return Unit.Value;
    }
}