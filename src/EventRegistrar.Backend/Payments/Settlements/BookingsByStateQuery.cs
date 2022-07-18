using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Settlements;

public class BookingsByStateQuery : IRequest<IEnumerable<BankBookingDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool HideIgnored { get; set; }
    public bool HideSettled { get; set; }
    public bool HideIncoming { get; set; }
    public bool HideOutgoing { get; set; }
}

public class BookingsByStateQueryHandler : IRequestHandler<BookingsByStateQuery, IEnumerable<BankBookingDisplayItem>>
{
    private readonly IQueryable<Payment> _bankBookings;

    public BookingsByStateQueryHandler(IQueryable<Payment> bankBookings)
    {
        _bankBookings = bankBookings;
    }

    public async Task<IEnumerable<BankBookingDisplayItem>> Handle(BookingsByStateQuery query, CancellationToken cancellationToken)
    {
        var payments = await _bankBookings.Where(bbk => bbk.BankAccountStatementsFile!.EventId == query.EventId)
                                          .WhereIf(query.HideIgnored, bbk => !bbk.Ignore)
                                          .WhereIf(query.HideSettled, bbk => !bbk.Settled)
                                          .WhereIf(query.HideIncoming, bbk => bbk.CreditDebitType != CreditDebit.CRDT)
                                          .WhereIf(query.HideOutgoing, bbk => bbk.CreditDebitType != CreditDebit.DBIT)
                                          .Select(bbk => new BankBookingDisplayItem
                                                         {
                                                             Id = bbk.Id,
                                                             Typ = bbk.CreditDebitType,
                                                             Amount = bbk.Amount,
                                                             Charges = bbk.Charges,
                                                             Message = bbk.Message,
                                                             DebitorName = bbk.DebitorName,
                                                             CreditorName = bbk.CreditorName,
                                                             CreditorIban = bbk.CreditorIban,
                                                             Currency = bbk.Currency,
                                                             Reference = bbk.Reference,

                                                             AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                             BookingDate = bbk.BookingDate,
                                                             AmountRepaid = bbk.Repaid,
                                                             Settled = bbk.Settled,
                                                             PaymentSlipId = bbk.PaymentSlipId,
                                                             Ignore = bbk.Ignore
                                                         })
                                          .OrderByDescending(bbk => bbk.BookingDate)
                                          .ThenByDescending(bbk => bbk.Amount)
                                          .ToListAsync(cancellationToken);
        return payments;
    }
}

public class BankBookingDisplayItem
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