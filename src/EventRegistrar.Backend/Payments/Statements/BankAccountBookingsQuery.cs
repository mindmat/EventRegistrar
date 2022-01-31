using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class BankAccountBookingsQuery : IRequest<IEnumerable<BookingsOfDay>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class BankAccountBookingsQueryHandler : IRequestHandler<BankAccountBookingsQuery, IEnumerable<BookingsOfDay>>
{
    private readonly IQueryable<BankAccountBooking> _payments;

    public BankAccountBookingsQueryHandler(IQueryable<BankAccountBooking> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<BookingsOfDay>> Handle(BankAccountBookingsQuery query,
                                                         CancellationToken cancellationToken)
    {
        var payments = await _payments.Where(rpy => rpy.BankAccountStatementsFile!.EventId == query.EventId)
                                      .GroupBy(rpy => new { rpy.BookingDate, rpy.BankAccountStatementsFile!.Balance })
                                      .Select(grp => new BookingsOfDay
                                                     {
                                                         BookingDate = grp.Key.BookingDate,
                                                         BalanceAfter = grp.Key.Balance,
                                                         Bookings = grp.Select(rpy => new BankAccountBookingDisplayItem
                                                                                      {
                                                                                          Id = rpy.Id,
                                                                                          Typ = rpy.CreditDebitType,
                                                                                          Amount = rpy.Amount,
                                                                                          Charges = rpy.Charges,
                                                                                          AmountAssigned = rpy.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                          BookingDate = rpy.BookingDate,
                                                                                          Currency = rpy.Currency,
                                                                                          DebitorName = rpy.DebitorName,
                                                                                          CreditorName = rpy.CreditorName,
                                                                                          Message = rpy.Message,
                                                                                          Reference = rpy.Reference,
                                                                                          AmountRepaid = rpy.Repaid,
                                                                                          Ignore = rpy.Ignore,
                                                                                          Settled = rpy.Settled
                                                                                      })
                                                     })
                                      .OrderByDescending(rpy => rpy.BookingDate)
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
}