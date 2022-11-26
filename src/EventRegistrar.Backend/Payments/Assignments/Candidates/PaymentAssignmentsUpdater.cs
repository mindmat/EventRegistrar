using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Assignments.Candidates;

public class PaymentAssignmentsCalculator : ReadModelCalculator<PaymentAssignments>
{
    public override string QueryName => nameof(PaymentAssignmentsQuery);
    public override bool IsDateDependent => false;


    private readonly IQueryable<Payment> _payments;
    private readonly IQueryable<Registration> _registrations;

    public PaymentAssignmentsCalculator(IQueryable<Registration> registrations,
                                        IQueryable<Payment> payments)
    {
        _registrations = registrations;
        _payments = payments;
    }

    public override async Task<PaymentAssignments> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Id == rowId
                                                && pmt.PaymentsFile!.EventId == eventId)
                                     .Include(pmt => pmt.Incoming!.Assignments!)
                                     .ThenInclude(pas => pas.Registration)
                                     .Include(pmt => pmt.Outgoing!.Assignments!)
                                     .ThenInclude(pas => pas.Registration)
                                     .FirstAsync(cancellationToken);
        var message = payment.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            message = payment.Info;
        }

        var otherParty = string.Empty;
        var result = new PaymentAssignments();
        if (payment.Incoming != null)
        {
            otherParty = payment.Incoming.DebitorName;

            result.Type = PaymentType.Incoming;
            result.OpenAmount = payment.Amount
                              - payment.Incoming.Assignments!.Sum(asn => asn.PayoutRequestId == null && asn.OutgoingPaymentId == null
                                                                             ? asn.Amount
                                                                             : -asn.Amount);

            result.ExistingAssignments = payment.Incoming
                                                .Assignments!
                                                .Where(pas => pas.Registration != null
                                                           && pas.PaymentAssignmentId_Counter == null)
                                                .Select(pas => new ExistingAssignment
                                                               {
                                                                   PaymentAssignmentId_Existing = pas.Id,
                                                                   PaymentId = payment.Id,
                                                                   RegistrationId = pas.RegistrationId!.Value,
                                                                   FirstName = pas.Registration!.RespondentFirstName,
                                                                   LastName = pas.Registration.RespondentLastName,
                                                                   Email = pas.Registration.RespondentEmail,
                                                                   IsWaitingList = pas.Registration.IsOnWaitingList == true,
                                                                   Price = pas.Registration.Price_AdmittedAndReduced,
                                                                   AssignedAmount = pas.Amount
                                                               })
                                                .ToList();
        }

        if (payment.Outgoing != null)
        {
            otherParty = payment.Outgoing.CreditorName;
            result.Type = PaymentType.Outgoing;
            result.OpenAmount = payment.Amount
                              - payment.Outgoing.Assignments!.Sum(asn => asn.Amount);

            result.ExistingAssignments = payment.Outgoing.Assignments!
                                                .Where(pas => pas.Registration != null
                                                           && pas.PaymentAssignmentId_Counter == null)
                                                .Select(pas => new ExistingAssignment
                                                               {
                                                                   PaymentAssignmentId_Existing = pas.Id,
                                                                   PaymentId = payment.Id,
                                                                   RegistrationId = pas.RegistrationId!.Value,
                                                                   FirstName = pas.Registration!.RespondentFirstName,
                                                                   LastName = pas.Registration.RespondentLastName,
                                                                   Email = pas.Registration.RespondentEmail,
                                                                   IsWaitingList = pas.Registration.IsOnWaitingList == true,
                                                                   Price = pas.Registration.Price_AdmittedAndReduced,
                                                                   AssignedAmount = pas.Amount
                                                               })
                                                .ToList();
        }

        if (result.OpenAmount != 0
         && (!string.IsNullOrWhiteSpace(message)
          || !string.IsNullOrWhiteSpace(otherParty)))
        {
            var registrations = await _registrations.Where(reg => reg.EventId == eventId)
                                                    .Where(reg => (message != null && reg.RespondentFirstName != null && message.Contains(reg.RespondentFirstName!))
                                                               || (message != null && reg.RespondentLastName != null && message.Contains(reg.RespondentLastName!))
                                                               || (message != null && reg.RespondentEmail != null && message.Contains(reg.RespondentEmail!))
                                                               || (otherParty != null && reg.RespondentFirstName != null && otherParty.Contains(reg.RespondentFirstName!))
                                                               || (otherParty != null && reg.RespondentLastName != null && otherParty.Contains(reg.RespondentLastName!)))
                                                    .Select(reg => new AssignmentCandidateRegistration
                                                                   {
                                                                       PaymentId = payment.Id,
                                                                       RegistrationId = reg.Id,
                                                                       FirstName = reg.RespondentFirstName,
                                                                       LastName = reg.RespondentLastName,
                                                                       Email = reg.RespondentEmail,
                                                                       Price = reg.Price_AdmittedAndReduced,
                                                                       AmountPaid = reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null && asn.OutgoingPaymentId == null
                                                                                                                           ? asn.Amount
                                                                                                                           : -asn.Amount),
                                                                       IsWaitingList = reg.IsOnWaitingList == true,
                                                                       State = reg.State
                                                                   })
                                                    .WhereIf(result.Type == PaymentType.Incoming, reg => reg.Price > reg.AmountPaid)
                                                    .WhereIf(result.Type == PaymentType.Outgoing, reg => reg.AmountPaid > 0)
                                                    .ToListAsync(cancellationToken);

            var wordsInPayment = message?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(wrd => wrd.ToLowerInvariant())
                                        .ToHashSet();

            registrations.ForEach(reg =>
            {
                reg.MatchScore = CalculateMatchScore(reg, wordsInPayment, otherParty);
                reg.AmountMatch = result.OpenAmount == reg.Price - reg.AmountPaid;
            });
            result.RegistrationCandidates = registrations.Where(mat => mat.MatchScore > 0)
                                                         .OrderByDescending(mtc => mtc.MatchScore);
        }

        return result;
    }

    private static int CalculateMatchScore(AssignmentCandidateRegistration reg,
                                           IReadOnlySet<string>? wordsInPayment,
                                           string? otherParty)
    {
        if (wordsInPayment == null)
        {
            return 0;
        }

        // names can contain multiple words, e.g. 'de Luca'
        var nameWords = (reg.FirstName?.Split(' ') ?? Enumerable.Empty<string>())
                        .Union(reg.LastName?.Split(' ') ?? Enumerable.Empty<string>())
                        .Select(nmw => nmw.ToLowerInvariant())
                        .ToList();
        var score = nameWords.Sum(nmw => wordsInPayment.Count(wrd => wrd == nmw));
        if (reg.Email != null)
        {
            score += wordsInPayment.Contains(reg.Email) ? 5 : 0;
        }

        if (otherParty != null)
        {
            var wordsInDebitor = otherParty.Split(' ')
                                           .Select(nmw => nmw.ToLowerInvariant())
                                           .ToList();

            score += nameWords.Sum(nmw => wordsInDebitor.Count(wrd => wrd == nmw));
        }

        return score;
    }
}

public class UpdatePaymentAssignmentsCommandWhenAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>,
                                                           IEventToCommandTranslation<OutgoingPaymentUnassigned>,
                                                           IEventToCommandTranslation<IncomingPaymentUnassigned>,
                                                           IEventToCommandTranslation<IncomingPaymentAssigned>


{
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdatePaymentAssignmentsCommandWhenAssigned(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(PaymentAssignmentsQuery),
                             EventId = e.EventId.Value,
                             RowId = e.OutgoingPaymentId,
                             DirtyMoment = _dateTimeProvider.Now
                         };
        }
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(PaymentAssignmentsQuery),
                             EventId = e.EventId.Value,
                             RowId = e.OutgoingPaymentId,
                             DirtyMoment = _dateTimeProvider.Now
                         };
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(PaymentAssignmentsQuery),
                             EventId = e.EventId.Value,
                             RowId = e.IncomingPaymentId,
                             DirtyMoment = _dateTimeProvider.Now
                         };
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdateReadModelCommand
                         {
                             QueryName = nameof(PaymentAssignmentsQuery),
                             EventId = e.EventId.Value,
                             RowId = e.IncomingPaymentId,
                             DirtyMoment = _dateTimeProvider.Now
                         };
        }
    }
}

public class PaymentAssignments
{
    public decimal OpenAmount { get; set; }
    public PaymentType Type { get; set; }
    public IEnumerable<AssignmentCandidateRegistration>? RegistrationCandidates { get; set; }
    public IEnumerable<ExistingAssignment>? ExistingAssignments { get; set; }
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
    public Guid PaymentId { get; set; }
    public RegistrationState State { get; set; }
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

    public Guid PaymentId { get; set; }
}