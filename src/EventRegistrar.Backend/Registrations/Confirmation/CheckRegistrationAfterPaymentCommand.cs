using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Confirmation;

public class CheckRegistrationAfterPaymentCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class CheckRegistrationAfterPaymentCommandHandler : AsyncRequestHandler<CheckRegistrationAfterPaymentCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IQueryable<Registration> _registrations;

    public CheckRegistrationAfterPaymentCommandHandler(IQueryable<Registration> registrations,
                                                       IEventBus eventBus)
    {
        _registrations = registrations;
        _eventBus = eventBus;
    }

    protected override async Task Handle(CheckRegistrationAfterPaymentCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                               .Include(reg => reg.PaymentAssignments)
                                               .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
        if (registration.IsOnWaitingList == true)
        {
            return;
        }

        var difference = registration.Price_AdmittedAndReduced
                       - registration.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
                                                                         ? asn.Amount
                                                                         : -asn.Amount);
        if (registration.State == RegistrationState.Received
         && (difference <= 0m || registration.WillPayAtCheckin))
        {
            // fully paid
            registration.State = RegistrationState.Paid;
            if (registration.RegistrationId_Partner == null)
            {
                _eventBus.Publish(new SingleRegistrationPaid
                                  {
                                      Id = Guid.NewGuid(),
                                      EventId = registration.EventId,
                                      RegistrationId = registration.Id,
                                      WillPayAtCheckin = difference > 0m
                                                      && registration.WillPayAtCheckin
                                  });
            }
            else
            {
                var partnerRegistration = await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner)
                                                              .Include(reg => reg.PaymentAssignments)
                                                              .FirstAsync(cancellationToken);
                var partnerRegistrationAccepted = partnerRegistration.State == RegistrationState.Paid && registration.IsOnWaitingList == false;
                if (partnerRegistrationAccepted)
                {
                    _eventBus.Publish(new PartnerRegistrationPaid
                                      {
                                          Id = Guid.NewGuid(),
                                          EventId = registration.EventId,
                                          RegistrationId1 = registration.Id,
                                          RegistrationId2 = partnerRegistration.Id,
                                          WillPayAtCheckin = difference > 0m
                                                          && registration.WillPayAtCheckin
                                      });
                }
                else
                {
                    _eventBus.Publish(new PartnerRegistrationPartiallyPaid
                                      {
                                          Id = Guid.NewGuid(),
                                          EventId = registration.EventId,
                                          RegistrationId1 = registration.Id,
                                          RegistrationId2 = partnerRegistration.Id
                                      });
                }
            }
        }
        else if (difference > 0
              && registration.State == RegistrationState.Paid
              && !registration.WillPayAtCheckin)
        {
            // payment has been revoked
            registration.State = RegistrationState.Received;
        }
    }
}