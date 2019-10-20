using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments
{
    internal class IgnorePaymentCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid PaymentId { get; set; }
    }

    internal class IgnorePaymentCommandHandler : IRequestHandler<IgnorePaymentCommand>
    {
        private readonly IRepository<ReceivedPayment> _payments;

        public IgnorePaymentCommandHandler(IRepository<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<Unit> Handle(IgnorePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _payments.FirstAsync(pmt => pmt.Id == request.PaymentId
                                                         && pmt.PaymentFile.EventId == request.EventId);
            if (payment.Ignore)
            {
                return Unit.Value;
            }
            payment.Ignore = true;

            return Unit.Value;
        }
    }
}