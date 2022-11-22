namespace EventRegistrar.Backend.Payments.Refunds;

public class PayoutQuery : IRequest<IEnumerable<PayoutDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PayoutQueryHandler : IRequestHandler<PayoutQuery, IEnumerable<PayoutDisplayItem>>
{
    private readonly IQueryable<PayoutRequest> _payoutRequests;

    public PayoutQueryHandler(IQueryable<PayoutRequest> payoutRequests)
    {
        _payoutRequests = payoutRequests;
    }

    public async Task<IEnumerable<PayoutDisplayItem>> Handle(PayoutQuery query, CancellationToken cancellationToken)
    {
        var payouts = await _payoutRequests.Where(por => por.Registration!.EventId == query.EventId)
                                           .Select(por => new PayoutDisplayItem
                                                          {
                                                              RegistrationId = por.RegistrationId,
                                                              Amount = por.Amount,
                                                              FirstName = por.Registration!.RespondentFirstName,
                                                              LastName = por.Registration.RespondentLastName,
                                                              Price = por.Registration.Price_AdmittedAndReduced,
                                                              Paid = por.Registration.PaymentAssignments!.Sum(ass => ass.PayoutRequestId == null
                                                                                                                         ? ass.Amount
                                                                                                                         : -ass.Amount),
                                                              Payments = por.Registration
                                                                            .PaymentAssignments!
                                                                            .Where(ass => ass.IncomingPayment != null)
                                                                            .Select(pmt => new PaymentDisplayItem
                                                                                           {
                                                                                               Assigned = pmt.Amount,
                                                                                               PaymentAmount = pmt.IncomingPayment!.Payment!.Amount,
                                                                                               PaymentBookingDate = pmt.IncomingPayment.Payment.BookingDate,
                                                                                               PaymentDebitorIban = pmt.IncomingPayment.DebitorIban,
                                                                                               PaymentDebitorName = pmt.IncomingPayment.DebitorName,
                                                                                               PaymentMessage = pmt.IncomingPayment.Payment.Message,
                                                                                               PaymentInfo = pmt.IncomingPayment.Payment.Info
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
    public IEnumerable<PaymentDisplayItem> Payments { get; set; }
    public decimal Amount { get; set; }
    public string StateText { get; set; }
    public PayoutState State { get; set; }
}

public class PaymentDisplayItem
{
    public decimal Assigned { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentBookingDate { get; set; }
    public string? PaymentDebitorIban { get; set; }
    public string? PaymentDebitorName { get; set; }
    public string? PaymentMessage { get; set; }
    public string? PaymentInfo { get; set; }
}