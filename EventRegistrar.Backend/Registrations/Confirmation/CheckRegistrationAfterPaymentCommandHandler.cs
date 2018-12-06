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
                             - registration.Payments.Sum(pmt => pmt.Amount)
                             - registration.IndividualReductions.Sum(idr => idr.Amount);
            if (difference <= 0m && registration.State == RegistrationState.Received)
            {
                // fully paid
                registration.State = RegistrationState.Paid;
                if (!registration.RegistrationId_Partner.HasValue)
                {
                    _eventBus.Publish(new SingleRegistrationPaid { Id = Guid.NewGuid(), RegistrationId = registration.Id });
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
                            RegistrationId1 = registration.Id,
                            RegistrationId2 = partnerRegistration.Id
                        });
                    }
                    else
                    {
                        _eventBus.Publish(new PartnerRegistrationPartiallyPaid
                        {
                            Id = Guid.NewGuid(),
                            RegistrationId1 = registration.Id,
                            RegistrationId2 = partnerRegistration.Id
                        });
                    }
                }
            }
            else if (difference > 0 && registration.State == RegistrationState.Paid)
            {
                // payment has been revoked
                registration.State = RegistrationState.Received;
            }

            return Unit.Value;
        }
    }
}