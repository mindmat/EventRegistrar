using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Differences;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Cancel;

public class CancelRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool DespitePayments { get; set; }
    public string? Reason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTimeOffset? Received { get; set; }
}

public class CancelRegistrationCommandHandler(IQueryable<Registration> registrations,
                                              IQueryable<Seat> _spots,
                                              IRepository<RegistrationCancellation> cancellations,
                                              IRepository<PayoutRequest> payoutRequests,
                                              SpotManager spotManager,
                                              IEventBus eventBus,
                                              IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CancelRegistrationCommand>
{
    public async Task Handle(CancelRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .Include(reg => reg.PaymentAssignments!)
                                              .ThenInclude(pas => pas.IncomingPayment)
                                              .Include(reg => reg.RegistrationForm)
                                              .Include(reg => reg.Mails!)
                                              .ThenInclude(mtr => mtr.Mail)
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);

        var paidAmount = registration.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
                                                                         ? asn.Amount
                                                                         : -asn.Amount);
        if ((paidAmount > 0 || registration is { Price_AdmittedAndReduced: > 0, State: RegistrationState.Paid }) && !command.DespitePayments)
        {
            throw new ApplicationException(string.Format(Resources.NotCancellableRegistrationHasPayments, registration.RespondentFirstName, registration.RespondentLastName, command.RegistrationId));
        }

        if (registration.State == RegistrationState.Cancelled)
        {
            throw new ApplicationException(string.Format(Resources.RegistrationAlreadyCancelled, registration.RespondentFirstName, registration.RespondentLastName, command.RegistrationId));
        }

        if (paidAmount > 0 && (command.RefundAmount is null or < 0 || command.RefundAmount > paidAmount))
        {
            throw new ApplicationException(string.Format(Resources.InvalidRefundAmount, command.RefundAmount, paidAmount));
        }

        registration.State = RegistrationState.Cancelled;
        registration.RegistrationId_Partner = null; // remove reference to partner

        // cancel spots
        var spots = await _spots.Where(plc => plc.RegistrationId == command.RegistrationId
                                           || plc.RegistrationId_Follower == command.RegistrationId)
                                .ToListAsync(cancellationToken);
        foreach (var spot in spots)
        {
            spotManager.RemoveSpot(spot, command.RegistrationId, RemoveSpotReason.CancellationOfRegistration);
        }

        // discard unsent mails
        foreach (var unsentMail in registration.Mails!.Where(mtr => mtr.Mail!.Withhold
                                                                 && !mtr.Mail.Discarded
                                                                 && mtr.Mail.Sent == null))
        {
            unsentMail.Mail!.Discarded = true;
        }

        var cancellation = new RegistrationCancellation
                           {
                               Id = Guid.NewGuid(),
                               RegistrationId = command.RegistrationId,
                               Reason = command.Reason,
                               Created = dateTimeProvider.Now,
                               RefundPercentage = paidAmount > 0 && command.RefundAmount != null
                                                      ? command.RefundAmount.Value / paidAmount
                                                      : 0,
                               Refund = command.RefundAmount ?? 0,
                               Received = command.Received
                           };
        cancellations.InsertObjectTree(cancellation);

        if (cancellation.Refund > 0m)
        {
            var payoutRequest = new PayoutRequest
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = command.RegistrationId,
                                    Amount = cancellation.Refund,
                                    Reason = command.Reason,
                                    State = PayoutState.Requested,
                                    Created = dateTimeProvider.Now,
                                    IbanProposed = registration.PaymentAssignments!
                                                               .Select(pas => pas.IncomingPayment?.DebitorIban)
                                                               .FirstOrDefault(iban => iban != null)
                                };
            payoutRequests.InsertObjectTree(payoutRequest);
        }

        eventBus.Publish(new RegistrationCancelled
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = command.RegistrationId,
                             EventId = registration.EventId,
                             Reason = command.Reason,
                             Refund = cancellation.Refund,
                             Received = command.Received ?? dateTimeProvider.Now,
                             Participant = $"{registration.RespondentFirstName} {registration.RespondentLastName}"
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(DifferencesQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(CancellationsQuery)
                         });
    }
}