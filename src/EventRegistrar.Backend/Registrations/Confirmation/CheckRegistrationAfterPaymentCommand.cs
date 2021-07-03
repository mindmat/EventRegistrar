using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class CheckRegistrationAfterPaymentCommand : IRequest
    {
        public Guid RegistrationId { get; set; }
    }

    public class CheckRegistrationAfterPaymentCommandHandler : IRequestHandler<CheckRegistrationAfterPaymentCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IQueryable<Registration> _registrations;

        public CheckRegistrationAfterPaymentCommandHandler(IRepository<Registration> registrations,
                                                           IEventBus eventBus)
        {
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(CheckRegistrationAfterPaymentCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Payments)
                                                   .Include(reg => reg.IndividualReductions)
                                                   .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
            if (registration.IsWaitingList == true)
            {
                return Unit.Value;
            }

            var difference = registration.Price
                           - registration.Payments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);
            if (registration.State == RegistrationState.Received
             && (difference <= 0m || registration.WillPayAtCheckin))
            {
                // fully paid
                registration.State = RegistrationState.Paid;
                if (!registration.RegistrationId_Partner.HasValue)
                {
                    _eventBus.Publish(new SingleRegistrationPaid
                    {
                        Id = Guid.NewGuid(),
                        EventId = registration.EventId,
                        RegistrationId = registration.Id,
                        WillPayAtCheckin = difference > 0m && registration.WillPayAtCheckin
                    });
                }
                else
                {
                    var partnerRegistration = await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner.Value)
                                                                  .Include(reg => reg.Payments)
                                                                  .Include(reg => reg.IndividualReductions)
                                                                  .FirstAsync(cancellationToken);
                    var partnerRegistrationAccepted = partnerRegistration.State == RegistrationState.Paid &&
                                                      registration.IsWaitingList == false;
                    if (partnerRegistrationAccepted)
                    {
                        _eventBus.Publish(new PartnerRegistrationPaid
                        {
                            Id = Guid.NewGuid(),
                            EventId = registration.EventId,
                            RegistrationId1 = registration.Id,
                            RegistrationId2 = partnerRegistration.Id,
                            WillPayAtCheckin = difference > 0m && registration.WillPayAtCheckin
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

            return Unit.Value;
        }
    }
}