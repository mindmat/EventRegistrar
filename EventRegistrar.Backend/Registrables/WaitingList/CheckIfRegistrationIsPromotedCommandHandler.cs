using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsPromotedCommandHandler : IRequestHandler<CheckIfRegistrationIsPromotedCommand>
    {
        private readonly IQueryable<Registration> _registrations;
        private readonly ServiceBusClient _serviceBusClient;

        public CheckIfRegistrationIsPromotedCommandHandler(IQueryable<Registration> registrations,
                                                           ServiceBusClient serviceBusClient)
        {
            _registrations = registrations;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(CheckIfRegistrationIsPromotedCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Seats_AsLeader)
                                                   .Include(reg => reg.Seats_AsFollower)
                                                   .FirstAsync(cancellationToken);

            var isWaitingListBefore = registration.IsWaitingList == true;
            registration.IsWaitingList = registration.Seats_AsLeader.Any(spt => (spt.RegistrableId == command.RegistrationId ||
                                                                                 spt.RegistrationId_Follower == command.RegistrationId)
                                                                             && spt.IsWaitingList
                                                                             && !spt.IsCancelled)
                                      || registration.Seats_AsFollower.Any(spt => (spt.RegistrableId == command.RegistrationId ||
                                                                                   spt.RegistrationId_Follower == command.RegistrationId)
                                                                               && spt.IsWaitingList
                                                                               && !spt.IsCancelled);
            if (isWaitingListBefore && registration.IsWaitingList == false)
            {
                // non-core spots are also not on waiting list anymore
                foreach (var spot in registration.Seats_AsLeader.Where(st => !st.IsCancelled && st.IsWaitingList))
                {
                    spot.IsWaitingList = false;
                }
                foreach (var spt in registration.Seats_AsFollower.Where(st => !st.IsCancelled && st.IsWaitingList))
                {
                    spt.IsWaitingList = false;
                }

                if (registration.AdmittedAt == null)
                {
                    registration.AdmittedAt = DateTime.UtcNow;
                }

                // registration is now accepted, send Mail
                var mailType = registration.RegistrationId_Partner.HasValue
                               ? MailType.PartnerRegistrationMatchedAndAccepted
                               : MailType.SingleRegistrationAccepted;
                _serviceBusClient.SendMessage(new ComposeAndSendMailCommand { MailType = mailType, Withhold = true, RegistrationId = registration.Id });
            }

            return Unit.Value;
        }
    }
}