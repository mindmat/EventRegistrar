using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PaymentAssignmentsQuery : IRequest<PaymentAssignments>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
    public string? SearchString { get; set; }
}

public class PaymentAssignmentsQueryHandler : IRequestHandler<PaymentAssignmentsQuery, PaymentAssignments>
{
    private readonly IQueryable<Payment> _payments;
    private readonly IQueryable<PayoutRequest> _payoutRequests;
    private readonly IQueryable<Registration> _registrations;

    public PaymentAssignmentsQueryHandler(IQueryable<Registration> registrations,
                                          IQueryable<Payment> payments,
                                          IQueryable<PayoutRequest> payoutRequests)
    {
        _registrations = registrations;
        _payments = payments;
        _payoutRequests = payoutRequests;
    }

    public async Task<PaymentAssignments> Handle(PaymentAssignmentsQuery query,
                                                 CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Id == query.PaymentId
                                                && pmt.PaymentsFile!.EventId == query.EventId)
                                     .Include(pmt => pmt.Incoming!.Assignments!)
                                     .ThenInclude(pas => pas.Registration)
                                     .Include(pmt => pmt.Incoming!.Assignments!)
                                     .ThenInclude(pas => pas.OutgoingPayment!.Payment)
                                     .Include(pmt => pmt.Outgoing!.Assignments!)
                                     .ThenInclude(pas => pas.Registration)
                                     .Include(pmt => pmt.Outgoing!.Assignments!)
                                     .ThenInclude(pas => pas.PayoutRequest)
                                     .FirstAsync(cancellationToken);

        var message = payment.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            message = payment.Info;
        }

        var otherParty = string.Empty;
        var result = new PaymentAssignments { Ignored = payment.Ignore };
        var search = !string.IsNullOrWhiteSpace(query.SearchString);
        if (payment.Incoming != null)
        {
            // Incoming payment -> find registrations
            otherParty = payment.Incoming.DebitorName;
            result.Type = PaymentType.Incoming;
            result.OpenAmount = payment.Amount
                              - payment.Incoming.Assignments!.Sum(asn => asn.PayoutRequestId == null && asn.OutgoingPaymentId == null
                                                                             ? asn.Amount
                                                                             : -asn.Amount);

            result.ExistingAssignments = payment.Incoming
                                                .Assignments!
                                                .Where(pas => pas.RegistrationId != null
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

            result.AssignedRepayments = payment.Incoming
                                               .Assignments!
                                               .Where(pas => pas.RegistrationId == null
                                                          && pas.OutgoingPaymentId != null)
                                               .Select(pas => new AssignedRepayment
                                                              {
                                                                  PaymentAssignmentId = pas.Id,
                                                                  CreditorName = pas.OutgoingPayment!.CreditorName,
                                                                  CreditorIban = pas.OutgoingPayment!.CreditorIban,
                                                                  AssignedAmount = pas.Amount,
                                                                  PaymentDate = pas.OutgoingPayment.Payment!.BookingDate
                                                              })
                                               .ToList();
            // find repayment candidates
            var payments = await _payments.Where(pmt => pmt.PaymentsFile!.EventId == query.EventId
                                                     && !pmt.Settled
                                                     && pmt.Type == PaymentType.Outgoing)
                                          .WhereIf(search, pmt => EF.Functions.Like(pmt.Outgoing!.CreditorName!, $"%{query.SearchString}%"))
                                          .Select(pmt => new RepaymentCandidate
                                                         {
                                                             PaymentId_Incoming = payment.Id,
                                                             PaymentId_Outgoing = pmt.Id,
                                                             BookingDate = pmt.BookingDate,
                                                             Amount = pmt.Amount,
                                                             AmountUnsettled = pmt.Amount
                                                                             - pmt.Outgoing!.Assignments!
                                                                                  .Select(asn => asn.PayoutRequestId == null
                                                                                                     ? asn.Amount
                                                                                                     : -asn.Amount)
                                                                                  .Sum(),
                                                             Settled = pmt.Settled,
                                                             Currency = pmt.Currency,
                                                             Info = pmt.Info,
                                                             CreditorName = pmt.Outgoing.CreditorName
                                                         })
                                          .ToListAsync(cancellationToken);
            if (!search)
            {
                payments.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment.Incoming));
                result.RepaymentCandidates = payments.Where(pmt => pmt.MatchScore > 1)
                                                     .OrderByDescending(mtc => mtc.MatchScore);
            }
            else
            {
                result.RepaymentCandidates = payments.OrderByDescending(mtc => mtc.MatchScore);
            }
        }

        if (payment.Outgoing != null)
        {
            // Outgoing payment -> find repayments & payout requests
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

            var payoutRequestCandidates = await _payoutRequests.Where(prq => prq.Registration!.EventId == query.EventId
                                                                          && prq.State != PayoutState.Confirmed)
                                                               .WhereIf(search, pmt => EF.Functions.Like(pmt.Registration!.RespondentFirstName!, $"%{query.SearchString}%")
                                                                                    || EF.Functions.Like(pmt.Registration!.RespondentLastName!, $"%{query.SearchString}%"))
                                                               .Select(prq => new PayoutRequestCandidate
                                                                              {
                                                                                  PayoutRequestId = prq.Id,
                                                                                  Participant = $"{prq.Registration!.RespondentFirstName} {prq.Registration.RespondentLastName}",
                                                                                  Amount = prq.Amount,
                                                                                  AmountUnsettled = prq.Amount - prq.Assignments!.Sum(pas => pas.Amount),
                                                                                  Info = prq.Reason,
                                                                                  IbanProposed = prq.IbanProposed
                                                                              })
                                                               .ToListAsync(cancellationToken);
            if (!search)
            {
                payoutRequestCandidates.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment.Outgoing));
                result.PayoutRequestCandidates = payoutRequestCandidates.Where(mat => mat.MatchScore > 1)
                                                                        .OrderByDescending(mtc => mtc.MatchScore);
            }
            else
            {
                result.PayoutRequestCandidates = payoutRequestCandidates.OrderByDescending(mtc => mtc.MatchScore);
            }

            result.AssignedPayoutRequests = payment.Outgoing.Assignments!
                                                   .Where(pas => pas.Registration != null
                                                              && pas.PayoutRequestId != null)
                                                   .Select(pas => new AssignedPayoutRequest
                                                                  {
                                                                      PaymentAssignmentId = pas.Id,
                                                                      PayoutRequestId = pas.PayoutRequestId!.Value,
                                                                      Participant = $"{pas.Registration!.RespondentFirstName} {pas.Registration.RespondentLastName}",
                                                                      Amount = pas.Amount,
                                                                      Info = pas.PayoutRequest?.Reason
                                                                  })
                                                   .ToList();
        }

        if (result.OpenAmount != 0
         && (!string.IsNullOrWhiteSpace(message)
          || !string.IsNullOrWhiteSpace(otherParty)))
        {
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId)
                                                    .WhereIf(search, reg => EF.Functions.Like(reg.RespondentFirstName!, $"%{query.SearchString}%")
                                                                         || EF.Functions.Like(reg.RespondentLastName!, $"%{query.SearchString}%"))
                                                    .WhereIf(!search, reg => (message != null && reg.RespondentFirstName != null && message.Contains(reg.RespondentFirstName!))
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
            if (!search)
            {
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
            else
            {
                result.RegistrationCandidates = registrations.OrderByDescending(mtc => mtc.MatchScore);
            }
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

    private static int CalculateMatchScore(RepaymentCandidate paymentCandidate, IncomingPayment openPayment)
    {
        var creditorParts = paymentCandidate.CreditorName?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                            ?.Select(wrd => wrd.ToLowerInvariant())
                         ?? new List<string>();

        var wordsInCandidate = paymentCandidate.Info?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                               ?.Select(wrd => wrd.ToLowerInvariant())
                                               ?.ToList()
                            ?? new List<string>();

        var unsettledAmountInOpenPayment = openPayment.Payment!.Amount
                                         - openPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                   ? asn.Amount
                                                                                   : -asn.Amount);
        var wordsInOpenPayment = openPayment.Payment!.Message?
                                            .Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(wrd => wrd.ToLowerInvariant())
                                            .ToHashSet()
                              ?? new HashSet<string>(0);

        return wordsInOpenPayment.Sum(opw => wordsInCandidate.Count(cdw => cdw == opw))
             + (wordsInOpenPayment.Sum(opw => creditorParts.Count(cdw => cdw == opw)) * 10)
             + (paymentCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0);
    }

    private static int CalculateMatchScore(PayoutRequestCandidate payoutRequestCandidate, OutgoingPayment openPayment)
    {
        var creditorParts = payoutRequestCandidate.Participant?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                                  ?.Select(wrd => wrd.ToLowerInvariant())
                         ?? new List<string>();

        var unsettledAmountInOpenPayment = openPayment.Payment!.Amount
                                         - openPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                   ? asn.Amount
                                                                                   : -asn.Amount);
        var wordsInOpenPayment = openPayment.Payment!.Message?
                                            .Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(wrd => wrd.ToLowerInvariant())
                                            .ToHashSet()
                              ?? new HashSet<string>(0);
        ;

        return (wordsInOpenPayment.Sum(opw => creditorParts.Count(cdw => cdw == opw)) * 10)
             + (payoutRequestCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0)
             + (payoutRequestCandidate.IbanProposed == openPayment.CreditorIban ? 50 : 0);
    }
}

public class UpdatePaymentAssignmentsCommandWhenAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>,
                                                           IEventToCommandTranslation<OutgoingPaymentUnassigned>,
                                                           IEventToCommandTranslation<IncomingPaymentUnassigned>,
                                                           IEventToCommandTranslation<IncomingPaymentAssigned>,
                                                           IEventToCommandTranslation<RegistrationCancelled>


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
            yield return CreateUpdateCommand(e.EventId!.Value, e.OutgoingPaymentId);
        }
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.OutgoingPaymentId);
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.IncomingPaymentId);
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.IncomingPaymentId);
        }
    }

    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, null);
        }
    }

    private UpdateReadModelCommand CreateUpdateCommand(Guid eventId, Guid? paymentId)
    {
        return new UpdateReadModelCommand
               {
                   QueryName = nameof(PaymentAssignmentsQuery),
                   EventId = eventId,
                   RowId = paymentId,
                   DirtyMoment = _dateTimeProvider.Now
               };
    }
}

public class PaymentAssignments
{
    public decimal OpenAmount { get; set; }
    public PaymentType Type { get; set; }
    public bool Ignored { get; set; }

    public IEnumerable<AssignmentCandidateRegistration>? RegistrationCandidates { get; set; }
    public IEnumerable<ExistingAssignment>? ExistingAssignments { get; set; }

    public IEnumerable<RepaymentCandidate>? RepaymentCandidates { get; set; }
    public IEnumerable<AssignedRepayment>? AssignedRepayments { get; set; }

    public IEnumerable<PayoutRequestCandidate>? PayoutRequestCandidates { get; set; }
    public IEnumerable<AssignedPayoutRequest>? AssignedPayoutRequests { get; set; }
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
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public decimal Price { get; set; }
    public bool IsWaitingList { get; set; }

    public Guid PaymentAssignmentId_Existing { get; set; }
    public decimal? AssignedAmount { get; set; }


    public Guid PaymentId { get; set; }
}

public class RepaymentCandidate
{
    public decimal Amount { get; set; }
    public decimal AmountUnsettled { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public string? CreditorName { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public Guid PaymentId_Outgoing { get; set; }
    public Guid PaymentId_Incoming { get; set; }
    public bool Settled { get; set; }
}

public class AssignedRepayment
{
    public Guid PaymentAssignmentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public decimal? AssignedAmount { get; set; }
}

public class PayoutRequestCandidate
{
    public Guid PayoutRequestId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountUnsettled { get; set; }
    public string? Participant { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public string? IbanProposed { get; set; }
}

public class AssignedPayoutRequest
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PayoutRequestId { get; set; }
    public decimal Amount { get; set; }
    public string? Participant { get; set; }
    public string? Info { get; set; }
}