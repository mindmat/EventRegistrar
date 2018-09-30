﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand>
    {
        private readonly IRepository<RegistrationCancellation> _cancellations;
        private readonly EventBus _eventBus;
        private readonly IQueryable<Registration> _registrations;
        private readonly SpotRemover _spotRemover;
        private readonly IQueryable<Seat> _spots;

        public CancelRegistrationCommandHandler(IQueryable<Registration> registrations,
                                                IQueryable<Seat> spots,
                                                IRepository<RegistrationCancellation> cancellations,
                                                SpotRemover spotRemover,
                                                EventBus eventBus)
        {
            _registrations = registrations;
            _spots = spots;
            _cancellations = cancellations;
            _spotRemover = spotRemover;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(CancelRegistrationCommand command, CancellationToken cancellationToken)
        {
            var refundPercentage = Math.Max(Math.Min(command.RefundPercentage, 1m), 0m);
            var registration = await _registrations.Include(reg => reg.Payments)
                                                   .Include(reg => reg.RegistrationForm)
                                                   .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);

            if (registration.Payments.Any() && !command.IgnorePayments)
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
            var spots = await _spots.Where(plc => plc.RegistrationId == command.RegistrationId
                                               || plc.RegistrationId_Follower == command.RegistrationId)
                                    .ToListAsync(cancellationToken);
            foreach (var spot in spots)
            {
                _spotRemover.RemoveSpot(spot, command.RegistrationId);
            }

            var cancellation = new RegistrationCancellation
            {
                Id = Guid.NewGuid(),
                RegistrationId = command.RegistrationId,
                Reason = command.Reason,
                Created = DateTime.UtcNow,
                RefundPercentage = refundPercentage,
                Refund = refundPercentage * registration.Payments.Sum(ass => ass.Amount)
            };
            await _cancellations.InsertOrUpdateEntity(cancellation, cancellationToken);

            _eventBus.Publish(new RegistrationCancelled { RegistrationId = command.RegistrationId });

            return Unit.Value;
        }
    }
}