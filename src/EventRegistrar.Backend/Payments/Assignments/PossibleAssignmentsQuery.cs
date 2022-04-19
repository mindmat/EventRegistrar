using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PossibleAssignmentsQuery : IRequest<BookingAssignments>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid BankAccountBookingId { get; set; }
}

public class BookingAssignments
{
    public IEnumerable<AssignmentCandidateRegistration>? RegistrationCandidates { get; set; }
    public IEnumerable<ExistingAssignment>? ExistingAssignments { get; set; }
}

public class PossibleAssignmentsQueryHandler : IRequestHandler<PossibleAssignmentsQuery, BookingAssignments>
{
    private readonly IQueryable<BankAccountBooking> _bookings;
    private readonly IQueryable<Registration> _registrations;

    public PossibleAssignmentsQueryHandler(IQueryable<BankAccountBooking> bookings,
                                           IQueryable<Registration> registrations)
    {
        _bookings = bookings;
        _registrations = registrations;
    }

    public async Task<BookingAssignments> Handle(PossibleAssignmentsQuery query,
                                                 CancellationToken cancellationToken)
    {
        var result = new BookingAssignments();
        var booking = await _bookings.Where(pmt => pmt.Id == query.BankAccountBookingId
                                                && pmt.BankAccountStatementsFile!.EventId == query.EventId)
                                     .Include(pmt => pmt.Assignments!)
                                     .ThenInclude(pas => pas.Registration)
                                     .FirstAsync(cancellationToken);
        var info = booking.Info;
        var openAmount = booking.Amount
                       - booking.Assignments!.Sum(asn => asn.PayoutRequestId == null
                             ? asn.Amount
                             : -asn.Amount);

        result.ExistingAssignments = booking.Assignments!
                                            .Where(pas => pas.Registration != null
                                                       && pas.PaymentAssignmentId_Counter == null)
                                            .Select(pas => new ExistingAssignment
                                                           {
                                                               PaymentAssignmentId_Existing = pas.Id,
                                                               BankAccountBookingId = booking.Id,
                                                               RegistrationId = pas.RegistrationId!.Value,
                                                               FirstName = pas.Registration!.RespondentFirstName,
                                                               LastName = pas.Registration.RespondentLastName,
                                                               Email = pas.Registration.RespondentEmail,
                                                               IsWaitingList = pas.Registration.IsWaitingList == true,
                                                               Price = pas.Registration.Price ?? 0m,
                                                               AssignedAmount = pas.Amount
                                                           })
                                            .ToList();
        if (openAmount == 0)
        {
            return result;
        }

        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                           && reg.State == RegistrationState.Received)
                                                .Where(reg => info!.Contains(reg.RespondentFirstName!)
                                                           || info.Contains(reg.RespondentLastName!)
                                                           || info.Contains(reg.RespondentEmail!))
                                                .Select(reg => new AssignmentCandidateRegistration
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

        registrations.ForEach(reg =>
        {
            reg.MatchScore = CalculateMatchScore(reg, wordsInPayment, openAmount);
            reg.AmountMatch = openAmount == reg.Price - reg.AmountPaid;
        });
        result.RegistrationCandidates = registrations.Where(mat => mat.MatchScore > 0)
                                                     .OrderByDescending(mtc => mtc.MatchScore);
        return result;
    }

    private static int CalculateMatchScore(AssignmentCandidateRegistration reg, HashSet<string>? wordsInPayment, decimal openAmount)
    {
        // names can contain multiple words, e.g. 'de Luca'
        var nameWords = reg.FirstName?.Split(' ')
                           .Union(reg.LastName?.Split(' '))
                           .Select(nmw => nmw.ToLowerInvariant())
                           .ToList();
        var nameScore = nameWords.Sum(nmw => wordsInPayment.Count(wrd => wrd == nmw));
        var mailaddressScore = wordsInPayment.Contains(reg.Email) ? 5 : 0;
        return nameScore + mailaddressScore;
    }
}

public class AssignmentCandidateRegistration
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

public class ExistingAssignment
{
    public Guid RegistrationId { get; set; }
    public Guid? PaymentAssignmentId_Existing { get; set; }
    public decimal? AssignedAmount { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public decimal Price { get; set; }
    public bool IsWaitingList { get; set; }

    public Guid BankAccountBookingId { get; set; }
}