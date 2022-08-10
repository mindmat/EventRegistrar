using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments.Candidates;

public class UpdatePaymentAssignmentsCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class UpdatePaymentAssignmentsCommandHandler : IRequestHandler<UpdatePaymentAssignmentsCommand>
{
    private readonly IQueryable<Payment> _payments;
    private readonly IQueryable<Registration> _registrations;

    private readonly IEventBus _eventBus;
    private readonly DbContext _dbContext;

    public UpdatePaymentAssignmentsCommandHandler(IQueryable<Registration> registrations,
                                                  DbContext dbContext,
                                                  IEventBus eventBus,
                                                  IQueryable<Payment> payments)
    {
        _registrations = registrations;
        _dbContext = dbContext;
        _eventBus = eventBus;
        _payments = payments;
    }

    public async Task<Unit> Handle(UpdatePaymentAssignmentsCommand command, CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Id == command.PaymentId
                                                && pmt.PaymentsFile!.EventId == command.EventId)
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
                                                                   IsWaitingList = pas.Registration.IsWaitingList == true,
                                                                   Price = pas.Registration.Price ?? 0m,
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
                                                                   IsWaitingList = pas.Registration.IsWaitingList == true,
                                                                   Price = pas.Registration.Price ?? 0m,
                                                                   AssignedAmount = pas.Amount
                                                               })
                                                .ToList();
        }

        if (result.OpenAmount != 0
         && (!string.IsNullOrWhiteSpace(message)
          || !string.IsNullOrWhiteSpace(otherParty)))
        {
            var registrations = await _registrations.Where(reg => reg.EventId == command.EventId)
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
                                                                       Price = reg.Price ?? 0m,
                                                                       AmountPaid = reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null && asn.OutgoingPaymentId == null
                                                                                                                           ? asn.Amount
                                                                                                                           : -asn.Amount),
                                                                       IsWaitingList = reg.IsWaitingList == true,
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

        var readModels = _dbContext.Set<PaymentAssignmentsReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rm => rm.EventId == command.EventId
                                                  && rm.PaymentId == command.PaymentId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel == null)
        {
            readModel = new PaymentAssignmentsReadModel
                        {
                            EventId = command.EventId,
                            PaymentId = command.PaymentId,
                            Content = result
                        };
            var entry = readModels.Attach(readModel);
            entry.State = EntityState.Added;
            _eventBus.Publish(new ReadModelUpdated
                              {
                                  EventId = command.EventId,
                                  QueryName = nameof(PaymentAssignmentsQuery),
                                  RowId = command.PaymentId
                              });
        }
        else
        {
            readModel.Content = result;
            if (_dbContext.Entry(readModel).State == EntityState.Modified)
            {
                _eventBus.Publish(new ReadModelUpdated
                                  {
                                      EventId = command.EventId,
                                      QueryName = nameof(PaymentAssignmentsQuery),
                                      RowId = command.PaymentId
                                  });
            }
        }

        return Unit.Value;
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
    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdatePaymentAssignmentsCommand
                         {
                             EventId = e.EventId.Value,
                             PaymentId = e.OutgoingPaymentId
                         };
        }
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdatePaymentAssignmentsCommand
                         {
                             EventId = e.EventId.Value,
                             PaymentId = e.OutgoingPaymentId
                         };
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdatePaymentAssignmentsCommand
                         {
                             EventId = e.EventId.Value,
                             PaymentId = e.IncomingPaymentId
                         };
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null)
        {
            yield return new UpdatePaymentAssignmentsCommand
                         {
                             EventId = e.EventId.Value,
                             PaymentId = e.IncomingPaymentId
                         };
        }
    }
}