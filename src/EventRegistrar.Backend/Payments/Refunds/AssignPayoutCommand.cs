using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds;

public class AssignPayoutCommand : IRequest, IEventBoundRequest
{
    public bool AcceptDifference { get; set; }
    public string? AcceptDifferenceReason { get; set; }
    public decimal Amount { get; set; }
    public Guid EventId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
    public Guid PayoutRequestId { get; set; }
}

public class AssignPayoutCommandHandler : IRequestHandler<AssignPayoutCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IQueryable<PayoutRequest> _payoutRequests;
    private readonly IQueryable<OutgoingPayment> _outgoingPayments;

    public AssignPayoutCommandHandler(IQueryable<PayoutRequest> payoutRequests,
                                      IQueryable<OutgoingPayment> outgoingPayments,
                                      IRepository<PaymentAssignment> assignments,
                                      IEventBus eventBus)
    {
        _payoutRequests = payoutRequests;
        _outgoingPayments = outgoingPayments;
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(AssignPayoutCommand command, CancellationToken cancellationToken)
    {
        var payoutRequest = await _payoutRequests.Where(reg => reg.Id == command.PayoutRequestId
                                                            && reg.Registration!.EventId == command.EventId)
                                                 .Include(reg => reg.Assignments)
                                                 .FirstAsync(cancellationToken);
        var outgoingPayment = await _outgoingPayments.FirstAsync(pmt => pmt.Id == command.OutgoingPaymentId, cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = payoutRequest.RegistrationId,
                             PayoutRequestId = payoutRequest.Id,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = command.Amount,
                             Created = DateTime.UtcNow
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        _eventBus.Publish(new OutgoingPaymentAssigned
                          {
                              PaymentAssignmentId = assignment.Id,
                              Amount = assignment.Amount,
                              PayoutRequestId = assignment.PayoutRequestId,
                              OutgoingPaymentId = outgoingPayment.Id,
                              RegistrationId = payoutRequest.RegistrationId
                          });

        return Unit.Value;
    }
}