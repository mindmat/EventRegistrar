using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Spots;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class CancelRegistrationCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public bool IgnorePayments { get; set; }
        public string Reason { get; set; }
        public decimal RefundPercentage { get; set; }
        public Guid RegistrationId { get; set; }
        public DateTime? Received { get; set; }
    }

    public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand>
    {
        private readonly IRepository<RegistrationCancellation> _cancellations;
        private readonly IRepository<PayoutRequest> _payoutRequests;
        private readonly IEventBus _eventBus;
        private readonly IQueryable<Registration> _registrations;
        private readonly SpotRemover _spotRemover;
        private readonly IQueryable<Seat> _spots;

        public CancelRegistrationCommandHandler(IQueryable<Registration> registrations,
                                                IQueryable<Seat> spots,
                                                IRepository<RegistrationCancellation> cancellations,
                                                IRepository<PayoutRequest> payoutRequests,
                                                SpotRemover spotRemover,
                                                IEventBus eventBus)
        {
            _registrations = registrations;
            _spots = spots;
            _cancellations = cancellations;
            _payoutRequests = payoutRequests;
            _spotRemover = spotRemover;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(CancelRegistrationCommand command, CancellationToken cancellationToken)
        {
            var refundPercentage = command.RefundPercentage > 1m
                ? command.RefundPercentage / 100m
                : command.RefundPercentage;
            refundPercentage = Math.Max(Math.Min(refundPercentage, 1m), 0m);

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
            registration.RegistrationId_Partner = null; // remove reference to partner
            var spots = await _spots.Where(plc => plc.RegistrationId == command.RegistrationId
                                               || plc.RegistrationId_Follower == command.RegistrationId)
                                    .ToListAsync(cancellationToken);
            foreach (var spot in spots)
            {
                _spotRemover.RemoveSpot(spot, command.RegistrationId, RemoveSpotReason.CancellationOfRegistration);
            }

            var cancellation = new RegistrationCancellation
            {
                Id = Guid.NewGuid(),
                RegistrationId = command.RegistrationId,
                Reason = command.Reason,
                Created = DateTime.UtcNow,
                RefundPercentage = refundPercentage,
                Refund = refundPercentage * registration.Payments.Sum(ass => ass.Amount),
                Received = command.Received
            };
            await _cancellations.InsertOrUpdateEntity(cancellation, cancellationToken);

            if (cancellation.Refund > 0m)
            {
                var payoutRequest = new PayoutRequest
                {
                    RegistrationId = command.RegistrationId,
                    Amount = cancellation.Refund,
                    Reason = command.Reason ?? "Refund after cancellation",
                    State = PayoutState.Requested,
                    Created = DateTimeOffset.Now
                };
                await _payoutRequests.InsertOrUpdateEntity(payoutRequest, cancellationToken);

            }

            _eventBus.Publish(new RegistrationCancelled
            {
                Id = Guid.NewGuid(),
                RegistrationId = command.RegistrationId,
                EventId = registration.EventId
            });

            return Unit.Value;
        }
    }
}