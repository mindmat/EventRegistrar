using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignedPaymentsOfRegistrationQuery : IRequest<IEnumerable<AssignedPaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class AssignedPaymentsOfRegistrationQueryHandler : IRequestHandler<AssignedPaymentsOfRegistrationQuery, IEnumerable<AssignedPaymentDisplayItem>>
{
    private readonly IQueryable<PaymentAssignment> _paymentsAssignments;

    public AssignedPaymentsOfRegistrationQueryHandler(IQueryable<PaymentAssignment> paymentsAssignments)
    {
        _paymentsAssignments = paymentsAssignments;
    }

    public async Task<IEnumerable<AssignedPaymentDisplayItem>> Handle(AssignedPaymentsOfRegistrationQuery query,
                                                                      CancellationToken cancellationToken)
    {
        return await _paymentsAssignments.Where(pya => pya.ReceivedPayment!.BankAccountStatementsFile!.EventId == query.EventId
                                                    && pya.RegistrationId == query.RegistrationId
                                                    && pya.ReceivedPayment.CreditDebitType == CreditDebit.CRDT)
                                         .Select(pya => new AssignedPaymentDisplayItem
                                                        {
                                                            PaymentAssignmentId = pya.Id,
                                                            Amount = pya.Amount,
                                                            Currency = pya.ReceivedPayment!.Currency,
                                                            BookingDate = pya.ReceivedPayment.BookingDate,
                                                            PaymentAssignmentId_Counter = pya.PaymentAssignmentId_Counter,
                                                            PaymentId_Repayment = pya.PaymentId_Repayment
                                                        })
                                         .ToListAsync(cancellationToken);
    }
}