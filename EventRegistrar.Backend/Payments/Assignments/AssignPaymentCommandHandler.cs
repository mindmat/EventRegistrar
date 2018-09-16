using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignPaymentCommandHandler : IRequestHandler<AssignPaymentCommand>
    {
        private readonly IRepository<PaymentAssignment> _assignments;
        private readonly IQueryable<ReceivedPayment> _payments;
        private readonly IQueryable<Registration> _registrations;

        public AssignPaymentCommandHandler(IQueryable<Registration> registrations,
            IQueryable<ReceivedPayment> payments,
            IRepository<PaymentAssignment> assignments)
        {
            _registrations = registrations;
            _payments = payments;
            _assignments = assignments;
        }

        public async Task<Unit> Handle(AssignPaymentCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistratrionId, cancellationToken);
            var payment = await _payments.FirstAsync(pmt => pmt.Id == command.PaymentId, cancellationToken);

            var assignment = new PaymentAssignment
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                ReceivedPaymentId = payment.Id,
                Amount = command.Amount
            };

            await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

            return Unit.Value;
        }
    }
}