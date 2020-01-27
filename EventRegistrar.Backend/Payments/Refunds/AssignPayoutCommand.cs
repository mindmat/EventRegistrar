using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Registrations.IndividualReductions;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Refunds
{
    public class AssignPayoutCommand : IRequest, IEventBoundRequest
    {
        public bool AcceptDifference { get; set; }
        public string AcceptDifferenceReason { get; set; }
        public decimal Amount { get; set; }
        public Guid EventId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid PayoutRequestId { get; set; }
    }

    public class AssignPayoutCommandHandler : IRequestHandler<AssignPayoutCommand>
    {
        private readonly IRepository<PaymentAssignment> _assignments;
        private readonly IEventBus _eventBus;
        private readonly IRepository<IndividualReduction> _individualReductions;
        private readonly IQueryable<PayoutRequest> _payoutRequests;
        private readonly IQueryable<ReceivedPayment> _payments;
        private readonly AuthenticatedUserId _userId;

        public AssignPayoutCommandHandler(IQueryable<PayoutRequest> payoutRequests,
                                          IQueryable<ReceivedPayment> payments,
                                          IRepository<PaymentAssignment> assignments,
                                          IRepository<IndividualReduction> individualReductions,
                                          IEventBus eventBus,
                                          AuthenticatedUserId userId)
        {
            _payoutRequests = payoutRequests;
            _payments = payments;
            _assignments = assignments;
            _individualReductions = individualReductions;
            _eventBus = eventBus;
            _userId = userId;
        }

        public async Task<Unit> Handle(AssignPayoutCommand command, CancellationToken cancellationToken)
        {
            var payoutRequest = await _payoutRequests.Where(reg => reg.Id == command.PayoutRequestId
                                                                && reg.Registration.EventId == command.EventId)
                                                     .Include(reg => reg.Assignments)
                                                     .FirstAsync(cancellationToken);
            var payment = await _payments.FirstAsync(pmt => pmt.Id == command.PaymentId, cancellationToken);

            var assignment = new PaymentAssignment
            {
                Id = Guid.NewGuid(),
                RegistrationId = payoutRequest.RegistrationId,
                PayoutRequestId = payoutRequest.Id,
                ReceivedPaymentId = payment.Id,
                Amount = command.Amount,
                Created = DateTime.UtcNow
            };
            await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

            //if (command.AcceptDifference)
            //{
            //    var difference = payoutRequest.Amount - payoutRequest.Assignments.Sum(pmt => pmt.Amount);
            //    await _individualReductions.InsertOrUpdateEntity(new IndividualReduction
            //    {
            //        Id = Guid.NewGuid(),
            //        RegistrationId = registration.Id,
            //        Amount = difference,
            //        Reason = command.AcceptDifferenceReason,
            //        UserId = _userId.UserId ?? Guid.Empty
            //    }, cancellationToken);

            //    _eventBus.Publish(new IndividualReductionAdded
            //    {
            //        RegistrationId = registration.Id,
            //        Amount = difference
            //    });
            //}

            _eventBus.Publish(new PaymentAssigned
            {
                Id = Guid.NewGuid(),
                PayoutRequestId = assignment.PayoutRequestId,
                PaymentId = assignment.ReceivedPaymentId
            });

            return Unit.Value;
        }
    }
}