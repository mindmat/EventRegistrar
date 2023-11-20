using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

namespace EventRegistrar.Backend.Payments.Statements;

public class PaymentsByDayQuery : IRequest<IEnumerable<BookingsOfDay>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool HideIgnored { get; set; }
    public bool HideSettled { get; set; }
    public bool HideIncoming { get; set; }
    public bool HideOutgoing { get; set; }
    public string? SearchString { get; set; }
}

public class PaymentsByDayQueryHandler(IQueryable<IncomingPayment> incomingBookings,
                                       IQueryable<OutgoingPayment> outgoingBookings)
    : IRequestHandler<PaymentsByDayQuery, IEnumerable<BookingsOfDay>>
{
    public async Task<IEnumerable<BookingsOfDay>> Handle(PaymentsByDayQuery query,
                                                         CancellationToken cancellationToken)
    {
        var payments = Enumerable.Empty<PaymentDisplayItem>();
        if (!query.HideIncoming)
        {
            payments = payments.Concat(await incomingBookings.Where(bbk => bbk.Payment!.EventId == query.EventId)
                                                             .WhereIf(query.HideIgnored, bbk => !bbk.Payment!.Ignore)
                                                             .WhereIf(query.HideSettled, bbk => !bbk.Payment!.Settled)
                                                             .WhereIf(!string.IsNullOrWhiteSpace(query.SearchString), bbk => EF.Functions.Like(bbk.Payment!.Message!, $"%{query.SearchString}%")
                                                                                                                          || EF.Functions.Like(bbk.DebitorName!, $"%{query.SearchString}%"))
                                                             .Select(bbk => new PaymentDisplayItem
                                                                            {
                                                                                Id = bbk.Id,
                                                                                Typ = CreditDebit.CRDT,
                                                                                Amount = bbk.Payment!.Amount,
                                                                                Charges = bbk.Payment!.Charges,
                                                                                Message = bbk.Payment!.Message,
                                                                                DebitorName = bbk.DebitorName,
                                                                                Currency = bbk.Payment!.Currency,
                                                                                Reference = bbk.Payment!.Reference,

                                                                                AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                BookingDate = bbk.Payment!.BookingDate,
                                                                                AmountRepaid = bbk.Payment!.Repaid,
                                                                                Settled = bbk.Payment!.Settled,
                                                                                PaymentSlipId = bbk.PaymentSlipId,
                                                                                Ignore = bbk.Payment!.Ignore,
                                                                                Balance = bbk.Payment!.PaymentsFile!.Balance
                                                                            })
                                                             .OrderByDescending(bbk => bbk.BookingDate)
                                                             .ThenByDescending(bbk => bbk.Amount)
                                                             .ToListAsync(cancellationToken));
        }

        if (!query.HideOutgoing)
        {
            payments = payments.Concat(await outgoingBookings.Where(bbk => bbk.Payment!.EventId == query.EventId)
                                                             .WhereIf(query.HideIgnored, bbk => !bbk.Payment!.Ignore)
                                                             .WhereIf(query.HideSettled, bbk => !bbk.Payment!.Settled)
                                                             .WhereIf(!string.IsNullOrWhiteSpace(query.SearchString), bbk => EF.Functions.Like(bbk.Payment!.Message!, $"%{query.SearchString}%")
                                                                                                                          || EF.Functions.Like(bbk.CreditorName!, $"%{query.SearchString}%"))
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
                                                                                Ignore = bbk.Payment.Ignore,
                                                                                Balance = bbk.Payment.PaymentsFile!.Balance
                                                                            })
                                                             .OrderByDescending(bbk => bbk.BookingDate)
                                                             .ThenByDescending(bbk => bbk.Amount)
                                                             .ToListAsync(cancellationToken));
        }

        return payments.GroupBy(pmt => new { pmt.BookingDate, pmt.Balance })
                       .Select(day => new BookingsOfDay
                                      {
                                          BookingDate = day.Key.BookingDate,
                                          BalanceAfter = day.Key.Balance,
                                          Bookings = day.Select(pmt => new PaymentDisplayItem
                                                                       {
                                                                           Id = pmt.Id,
                                                                           BookingDate = pmt.BookingDate,
                                                                           Typ = pmt.Typ,
                                                                           Currency = pmt.Currency,
                                                                           Amount = pmt.Amount,
                                                                           Charges = pmt.Charges,
                                                                           Message = pmt.Message,
                                                                           DebitorName = pmt.DebitorName,
                                                                           CreditorName = pmt.CreditorName,
                                                                           CreditorIban = pmt.CreditorIban,
                                                                           Reference = pmt.Reference,
                                                                           PaymentSlipId = pmt.PaymentSlipId,

                                                                           AmountAssigned = pmt.AmountAssigned,
                                                                           AmountRepaid = pmt.AmountRepaid,
                                                                           Ignore = pmt.Ignore,
                                                                           Settled = pmt.Settled
                                                                       })
                                      })
                       .OrderByDescending(day => day.BookingDate);
    }
}

public class BookingsOfDay
{
    public DateTime BookingDate { get; set; }
    public IEnumerable<PaymentDisplayItem> Bookings { get; set; } = null!;
    public decimal? BalanceAfter { get; set; }
}

public class PaymentDisplayItem
{
    public Guid Id { get; set; }
    public CreditDebit? Typ { get; set; }
    public decimal Amount { get; set; }
    public decimal? Charges { get; set; }
    public decimal AmountAssigned { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public string? Reference { get; set; }
    public decimal? AmountRepaid { get; set; }
    public bool Settled { get; set; }
    public bool Ignore { get; set; }
    public string? Message { get; set; }
    public string? DebitorName { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public Guid? PaymentSlipId { get; set; }
    public decimal? Balance { get; set; }
}