using EventRegistrar.Backend.Authorization;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignedPaymentsOfRegistrationQuery : IRequest<IEnumerable<AssignedPaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class AssignedPaymentsOfRegistrationQueryHandler : IRequestHandler<AssignedPaymentsOfRegistrationQuery, IEnumerable<AssignedPaymentDisplayItem>>
{
    private readonly IQueryable<PaymentAssignment> _assignments;

    public AssignedPaymentsOfRegistrationQueryHandler(IQueryable<PaymentAssignment> assignments)
    {
        _assignments = assignments;
    }

    public async Task<IEnumerable<AssignedPaymentDisplayItem>> Handle(AssignedPaymentsOfRegistrationQuery query,
                                                                      CancellationToken cancellationToken)
    {
        return await _assignments.Where(ass => ass.IncomingPayment!.Payment!.PaymentsFile!.EventId == query.EventId
                                            && ass.RegistrationId == query.RegistrationId)
                                 .Select(ass => new AssignedPaymentDisplayItem
                                                {
                                                    PaymentAssignmentId = ass.Id,
                                                    Amount = ass.Amount,
                                                    Currency = ass.IncomingPayment!.Payment!.Currency,
                                                    BookingDate = ass.IncomingPayment.Payment.BookingDate,
                                                    PaymentAssignmentId_Counter = ass.PaymentAssignmentId_Counter,
                                                    OutgoingPaymentId = ass.OutgoingPaymentId
                                                })
                                 .ToListAsync(cancellationToken);
    }
}