using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Settlements;

public class BookingsByStateQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool HideIgnored { get; set; }
    public bool HideSettled { get; set; }
    public bool HideIncoming { get; set; }
    public bool HideOutgoing { get; set; }
}

public class BookingsByStateQueryHandler : IRequestHandler<BookingsByStateQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<IncomingPayment> _incomingBookings;
    private readonly IQueryable<OutgoingPayment> _outgoingBookings;

    public BookingsByStateQueryHandler(IQueryable<IncomingPayment> incomingBookings,
                                       IQueryable<OutgoingPayment> outgoingBookings)
    {
        _incomingBookings = incomingBookings;
        _outgoingBookings = outgoingBookings;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(BookingsByStateQuery query, CancellationToken cancellationToken)
    {
        var payments = Enumerable.Empty<PaymentDisplayItem>();
        if (!query.HideIncoming)
        {
            payments = payments.Concat(await _incomingBookings.Where(bbk => bbk.Payment!.PaymentsFile!.EventId == query.EventId)
                                                              .WhereIf(query.HideIgnored, bbk => !bbk.Payment!.Ignore)
                                                              .WhereIf(query.HideSettled, bbk => !bbk.Payment!.Settled)
                                                              .Select(bbk => new PaymentDisplayItem
                                                                             {
                                                                                 Id = bbk.Id,
                                                                                 Typ = CreditDebit.CRDT,
                                                                                 Amount = bbk.Payment!.Amount,
                                                                                 Charges = bbk.Payment.Charges,
                                                                                 Message = bbk.Payment.Message,
                                                                                 DebitorName = bbk.DebitorName,
                                                                                 Currency = bbk.Payment.Currency,
                                                                                 Reference = bbk.Payment.Reference,

                                                                                 AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                 BookingDate = bbk.Payment.BookingDate,
                                                                                 AmountRepaid = bbk.Payment.Repaid,
                                                                                 Settled = bbk.Payment.Settled,
                                                                                 PaymentSlipId = bbk.PaymentSlipId,
                                                                                 Ignore = bbk.Payment.Ignore
                                                                             })
                                                              .OrderByDescending(bbk => bbk.BookingDate)
                                                              .ThenByDescending(bbk => bbk.Amount)
                                                              .ToListAsync(cancellationToken));
        }

        if (!query.HideOutgoing)
        {
            payments = payments.Concat(await _outgoingBookings.Where(bbk => bbk.Payment!.PaymentsFile!.EventId == query.EventId)
                                                              .WhereIf(query.HideIgnored, bbk => !bbk.Payment!.Ignore)
                                                              .WhereIf(query.HideSettled, bbk => !bbk.Payment!.Settled)
                                                              .Select(bbk => new PaymentDisplayItem
                                                                             {
                                                                                 Id = bbk.Id,
                                                                                 Typ = CreditDebit.DBIT,
                                                                                 Amount = bbk.Payment!.Amount,
                                                                                 Charges = bbk.Payment.Charges,
                                                                                 Message = bbk.Payment.Message,
                                                                                 CreditorName = bbk.CreditorName,
                                                                                 CreditorIban = bbk.CreditorIban,
                                                                                 Currency = bbk.Payment.Currency,
                                                                                 Reference = bbk.Payment.Reference,

                                                                                 AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                 BookingDate = bbk.Payment.BookingDate,
                                                                                 AmountRepaid = bbk.Payment.Repaid,
                                                                                 Settled = bbk.Payment.Settled,
                                                                                 Ignore = bbk.Payment.Ignore
                                                                             })
                                                              .OrderByDescending(bbk => bbk.BookingDate)
                                                              .ThenByDescending(bbk => bbk.Amount)
                                                              .ToListAsync(cancellationToken));
        }

        return payments.OrderByDescending(bbk => bbk.BookingDate)
                       .ThenByDescending(bbk => bbk.Amount);
    }
}

public class PaymentDisplayItem
{
    public Guid Id { get; set; }
    public CreditDebit? Typ { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal Amount { get; set; }
    public decimal? Charges { get; set; }
    public string? Currency { get; set; }
    public string? DebitorName { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public string? Message { get; set; }
    public string? Reference { get; set; }
    public Guid? PaymentSlipId { get; set; }

    public decimal AmountAssigned { get; set; }
    public decimal? AmountRepaid { get; set; }
    public bool Settled { get; set; }
    public bool Ignore { get; set; }
}