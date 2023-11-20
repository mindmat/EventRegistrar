namespace EventRegistrar.Backend.Payments.Refunds;

public class PayoutQuery : IRequest<IEnumerable<PayoutDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PayoutQueryHandler(IQueryable<PayoutRequest> payoutRequests) : IRequestHandler<PayoutQuery, IEnumerable<PayoutDisplayItem>>
{
    public async Task<IEnumerable<PayoutDisplayItem>> Handle(PayoutQuery query, CancellationToken cancellationToken)
    {
        var payouts = await payoutRequests.Where(por => por.Registration!.EventId == query.EventId)
                                          .Select(por => new PayoutDisplayItem
                                                         {
                                                             RegistrationId = por.RegistrationId,
                                                             Amount = por.Amount,
                                                             FirstName = por.Registration!.RespondentFirstName,
                                                             LastName = por.Registration.RespondentLastName,
                                                             Price = por.Registration.Price_AdmittedAndReduced,
                                                             Paid = por.Registration.PaymentAssignments!.Sum(ass => ass.OutgoingPayment == null
                                                                                                                        ? ass.Amount
                                                                                                                        : -ass.Amount),
                                                             IncomingPayments = por.Registration
                                                                                   .PaymentAssignments!
                                                                                   .Where(ass => ass.IncomingPayment != null)
                                                                                   .Select(pmt => new IncomingPaymentDisplayItem
                                                                                                  {
                                                                                                      Assigned = pmt.Amount,
                                                                                                      PaymentAmount = pmt.IncomingPayment!.Payment!.Amount,
                                                                                                      PaymentBookingDate = pmt.IncomingPayment.Payment.BookingDate,
                                                                                                      PaymentDebitorIban = pmt.IncomingPayment.DebitorIban,
                                                                                                      PaymentDebitorName = pmt.IncomingPayment.DebitorName,
                                                                                                      PaymentMessage = pmt.IncomingPayment.Payment.Message,
                                                                                                      PaymentInfo = pmt.IncomingPayment.Payment.Info
                                                                                                  }),
                                                             OutgoingPayments = por.Registration
                                                                                   .PaymentAssignments!
                                                                                   .Where(ass => ass.OutgoingPayment != null)
                                                                                   .Select(pmt => new OutgoingPaymentDisplayItem
                                                                                                  {
                                                                                                      Assigned = pmt.Amount,
                                                                                                      PaymentAmount = pmt.OutgoingPayment!.Payment!.Amount,
                                                                                                      PaymentBookingDate = pmt.OutgoingPayment.Payment.BookingDate,
                                                                                                      PaymentCreditorIban = pmt.OutgoingPayment.CreditorIban,
                                                                                                      PaymentCreditorName = pmt.OutgoingPayment.CreditorName,
                                                                                                      PaymentMessage = pmt.OutgoingPayment.Payment.Message,
                                                                                                      PaymentInfo = pmt.OutgoingPayment.Payment.Info
                                                                                                  }),
                                                             Reason = por.Reason,
                                                             StateText = por.State.ToString(),
                                                             State = por.State,
                                                             Created = por.Created
                                                         })
                                          .OrderByDescending(rpy => rpy.Created)
                                          .ToListAsync(cancellationToken);
        return payouts;
    }
}

public class PayoutDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal Price { get; set; }
    public decimal Paid { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset Created { get; set; }
    public IEnumerable<IncomingPaymentDisplayItem>? IncomingPayments { get; set; }
    public IEnumerable<OutgoingPaymentDisplayItem>? OutgoingPayments { get; set; }
    public decimal Amount { get; set; }
    public string? StateText { get; set; }
    public PayoutState State { get; set; }
}

public class IncomingPaymentDisplayItem
{
    public decimal Assigned { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentBookingDate { get; set; }
    public string? PaymentDebitorIban { get; set; }
    public string? PaymentDebitorName { get; set; }
    public string? PaymentMessage { get; set; }
    public string? PaymentInfo { get; set; }
}

public class OutgoingPaymentDisplayItem
{
    public decimal Assigned { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentBookingDate { get; set; }
    public string? PaymentCreditorIban { get; set; }
    public string? PaymentCreditorName { get; set; }
    public string? PaymentMessage { get; set; }
    public string? PaymentInfo { get; set; }
}