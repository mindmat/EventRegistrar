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
        var data = await _assignments.Where(ass => ass.RegistrationId == query.RegistrationId
                                                && ass.Registration!.EventId == query.EventId
                                                && ass.PaymentAssignmentId_Counter == null)
                                     .Select(ass => new
                                                    {
                                                        ass.Id,
                                                        ass.Amount,
                                                        ass.IncomingPaymentId,
                                                        ass.OutgoingPaymentId,
                                                        Currency_Incoming = (string?)ass.IncomingPayment!.Payment!.Currency,
                                                        BookingDate_Incoming = (DateTime?)ass.IncomingPayment.Payment.BookingDate,
                                                        Currency_Outgoing = (string?)ass.OutgoingPayment!.Payment!.Currency,
                                                        BookingDate_Outgoing = (DateTime?)ass.OutgoingPayment.Payment.BookingDate
                                                    })
                                     .ToListAsync(cancellationToken);
        return data.Where(ass => ass.IncomingPaymentId != null)
                   .Select(ass => new AssignedPaymentDisplayItem
                                  {
                                      PaymentAssignmentId = ass.Id,
                                      Amount = ass.Amount,
                                      Currency = ass.Currency_Incoming,
                                      BookingDate = ass.BookingDate_Incoming!.Value
                                  })
                   .Concat(data.Where(ass => ass.OutgoingPaymentId != null)
                               .Select(ass => new AssignedPaymentDisplayItem
                                              {
                                                  PaymentAssignmentId = ass.Id,
                                                  Amount = -ass.Amount,
                                                  Currency = ass.Currency_Outgoing,
                                                  BookingDate = ass.BookingDate_Outgoing!.Value
                                              }))
                   .ToList();
    }
}

public class AssignedPaymentDisplayItem
{
    public decimal Amount { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public Guid PaymentAssignmentId { get; set; }
}