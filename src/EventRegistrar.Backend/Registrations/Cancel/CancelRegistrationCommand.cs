using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Cancel;

public class CancelRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool IgnorePayments { get; set; }
    public string? Reason { get; set; }
    public decimal RefundPercentage { get; set; }
    public Guid RegistrationId { get; set; }
    public DateTimeOffset? Received { get; set; }
}

public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand>
{
    private readonly IRepository<RegistrationCancellation> _cancellations;
    private readonly IRepository<PayoutRequest> _payoutRequests;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<Registration> _registrations;
    private readonly SpotManager _spotManager;
    private readonly IQueryable<Seat> _spots;

    public CancelRegistrationCommandHandler(IQueryable<Registration> registrations,
                                            IQueryable<Seat> spots,
                                            IRepository<RegistrationCancellation> cancellations,
                                            IRepository<PayoutRequest> payoutRequests,
                                            SpotManager spotManager,
                                            IEventBus eventBus,
                                            IDateTimeProvider dateTimeProvider)
    {
        _registrations = registrations;
        _spots = spots;
        _cancellations = cancellations;
        _payoutRequests = payoutRequests;
        _spotManager = spotManager;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(CancelRegistrationCommand command, CancellationToken cancellationToken)
    {
        var refundPercentage = command.RefundPercentage > 1m
                                   ? command.RefundPercentage / 100m
                                   : command.RefundPercentage;
        refundPercentage = Math.Clamp(refundPercentage, 0m, 1m);

        var registration = await _registrations.AsTracking()
                                               .Include(reg => reg.PaymentAssignments)
                                               .Include(reg => reg.RegistrationForm)
                                               .Include(reg => reg.Mails!)
                                               .ThenInclude(mtr => mtr.Mail)
                                               .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);

        if (registration.PaymentAssignments!.Any() && !command.IgnorePayments)
        {
            throw new ApplicationException($"There are already payments for registration {command.RegistrationId}");
        }

        if (registration.State == RegistrationState.Cancelled)
        {
            throw new ApplicationException($"Registration {command.RegistrationId} is already cancelled");
        }

        if (registration.State == RegistrationState.Paid && !command.IgnorePayments)
        {
            throw new ApplicationException($"Registration {command.RegistrationId} is already paid and cannot be cancelled anymore");
        }

        registration.State = RegistrationState.Cancelled;
        registration.RegistrationId_Partner = null; // remove reference to partner

        // cancel spots
        var spots = await _spots.Where(plc => plc.RegistrationId == command.RegistrationId
                                           || plc.RegistrationId_Follower == command.RegistrationId)
                                .ToListAsync(cancellationToken);
        foreach (var spot in spots)
        {
            _spotManager.RemoveSpot(spot, command.RegistrationId, RemoveSpotReason.CancellationOfRegistration);
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
                               Created = _dateTimeProvider.Now,
                               RefundPercentage = refundPercentage,
                               Refund = refundPercentage
                                      * registration.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                               Received = command.Received
                           };
        _cancellations.InsertObjectTree(cancellation);

        if (cancellation.Refund > 0m)
        {
            var payoutRequest = new PayoutRequest
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = command.RegistrationId,
                                    Amount = cancellation.Refund,
                                    Reason = command.Reason,
                                    State = PayoutState.Requested,
                                    Created = _dateTimeProvider.Now
                                };
            _payoutRequests.InsertObjectTree(payoutRequest);
        }

        _eventBus.Publish(new RegistrationCancelled
                          {
                              Id = Guid.NewGuid(),
                              RegistrationId = command.RegistrationId,
                              EventId = registration.EventId,
                              Reason = command.Reason,
                              Refund = cancellation.Refund,
                              Received = command.Received ?? _dateTimeProvider.Now,
                              Participant = $"{registration.RespondentFirstName} {registration.RespondentLastName}"
                          });

        return Unit.Value;
    }
}