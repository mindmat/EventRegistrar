using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class BankAccountBookingsQuery : IRequest<IEnumerable<BookingsOfDay>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool HideIgnored { get; set; }
    public bool HideSettled { get; set; }
    public bool HideIncoming { get; set; }
    public bool HideOutgoing { get; set; }
    public string? SearchString { get; set; }
}

public class BankAccountBookingsQueryHandler : IRequestHandler<BankAccountBookingsQuery, IEnumerable<BookingsOfDay>>
{
    private readonly IQueryable<BankAccountBooking> _bankBookings;

    public BankAccountBookingsQueryHandler(IQueryable<BankAccountBooking> bankBookings)
    {
        _bankBookings = bankBookings;
    }

    public async Task<IEnumerable<BookingsOfDay>> Handle(BankAccountBookingsQuery query,
                                                         CancellationToken cancellationToken)
    {
        var payments = await _bankBookings.Where(bbk => bbk.BankAccountStatementsFile!.EventId == query.EventId)
                                          .WhereIf(query.HideIgnored, bbk => !bbk.Ignore)
                                          .WhereIf(query.HideSettled, bbk => !bbk.Settled)
                                          .WhereIf(query.HideIncoming, bbk => bbk.CreditDebitType != CreditDebit.CRDT)
                                          .WhereIf(query.HideOutgoing, bbk => bbk.CreditDebitType != CreditDebit.DBIT)
                                          .WhereIf(query.SearchString != null, bbk => EF.Functions.Like(bbk.Message!, $"%{query.SearchString}%")
                                                                                   || EF.Functions.Like(bbk.CreditorName!, $"%{query.SearchString}%")
                                                                                   || EF.Functions.Like(bbk.DebitorName!, $"%{query.SearchString}%"))
                                          .GroupBy(bbk => new { bbk.BookingDate, bbk.BankAccountStatementsFile!.Balance })
                                          .Select(day => new BookingsOfDay
                                                         {
                                                             BookingDate = day.Key.BookingDate,
                                                             BalanceAfter = day.Key.Balance,
                                                             Bookings = day.Select(bbk => new BankAccountBookingDisplayItem
                                                                                          {
                                                                                              Id = bbk.Id,
                                                                                              BookingDate = bbk.BookingDate,
                                                                                              Typ = bbk.CreditDebitType,
                                                                                              Currency = bbk.Currency,
                                                                                              Amount = bbk.Amount,
                                                                                              Charges = bbk.Charges,
                                                                                              Message = bbk.Message,
                                                                                              DebitorName = bbk.DebitorName,
                                                                                              CreditorName = bbk.CreditorName,
                                                                                              CreditorIban = bbk.CreditorIban,
                                                                                              Reference = bbk.Reference,
                                                                                              PaymentSlipId = bbk.PaymentSlipId,

                                                                                              AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                              AmountRepaid = bbk.Repaid,
                                                                                              Ignore = bbk.Ignore,
                                                                                              Settled = bbk.Settled
                                                                                          })
                                                         })
                                          .OrderByDescending(day => day.BookingDate)
                                          .ToListAsync(cancellationToken);
        return payments;
    }
}

public class BookingsOfDay
{
    public DateTime BookingDate { get; set; }
    public IEnumerable<BankAccountBookingDisplayItem> Bookings { get; set; } = null!;
    public decimal? BalanceAfter { get; set; }
}

public class BankAccountBookingDisplayItem
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
}