using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PossibleAssignmentsQuery : IRequest<IEnumerable<PossibleAssignment>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid BankAccountBookingId { get; set; }
}

public class PossibleAssignmentsQueryHandler : IRequestHandler<PossibleAssignmentsQuery, IEnumerable<PossibleAssignment>>
{
    private readonly IQueryable<BankAccountBooking> _bookings;
    private readonly IQueryable<Registration> _registrations;

    public PossibleAssignmentsQueryHandler(IQueryable<BankAccountBooking> bookings,
                                           IQueryable<Registration> registrations)
    {
        _bookings = bookings;
        _registrations = registrations;
    }

    public async Task<IEnumerable<PossibleAssignment>> Handle(PossibleAssignmentsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var booking = await _bookings.Where(pmt => pmt.Id == query.BankAccountBookingId
                                                && pmt.BankAccountStatementsFile!.EventId == query.EventId)
                                     .Include(pmt => pmt.Assignments)
                                     .FirstAsync(cancellationToken);
        var info = booking.Info;
        var openAmount = booking.Amount -
                         booking.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);

        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                           && reg.State == RegistrationState.Received)
                                                .Where(reg => info!.Contains(reg.RespondentFirstName!)
                                                           || info.Contains(reg.RespondentLastName!)
                                                           || info.Contains(reg.RespondentEmail!))
                                                .Select(reg => new PossibleAssignment
                                                               {
                                                                   BankAccountBookingId = booking.Id,
                                                                   RegistrationId = reg.Id,
                                                                   FirstName = reg.RespondentFirstName,
                                                                   LastName = reg.RespondentLastName,
                                                                   Email = reg.RespondentEmail,
                                                                   Price = reg.Price ?? 0m,
                                                                   AmountPaid = reg.Payments!.Sum(asn =>
                                                                       asn.PayoutRequestId == null
                                                                           ? asn.Amount
                                                                           : -asn.Amount),
                                                                   IsWaitingList = reg.IsWaitingList == true
                                                               })
                                                .ToListAsync(cancellationToken);

        var wordsInPayment = info?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(wrd => wrd.ToLowerInvariant())
                                 .ToHashSet();

        registrations.ForEach(reg => reg.MatchScore = CalculateMatchScore(reg, wordsInPayment, openAmount));
        registrations.ForEach(reg => reg.AmountMatch = openAmount == reg.Price - reg.AmountPaid);
        return registrations.Where(mat => mat.MatchScore > 0)
                            .OrderByDescending(mtc => mtc.MatchScore);
    }

    private static int CalculateMatchScore(PossibleAssignment reg, HashSet<string>? wordsInPayment, decimal openAmount)
    {
        // names can contain multiple words, e.g. 'de Luca'
        var nameWords = reg.FirstName.Split(' ')
                           .Union(reg.LastName.Split(' '))
                           .Select(nmw => nmw.ToLowerInvariant())
                           .ToList();
        var nameScore = nameWords.Sum(nmw => wordsInPayment.Count(wrd => wrd == nmw));
        var mailaddressScore = wordsInPayment.Contains(reg.Email) ? 5 : 0;
        return nameScore + mailaddressScore;
    }
}

public class PossibleAssignment
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public decimal Price { get; set; }
    public bool IsWaitingList { get; set; }

    public bool AmountMatch { get; set; }
    public decimal AmountPaid { get; set; }
    public int MatchScore { get; set; }
    public Guid BankAccountBookingId { get; set; }
}