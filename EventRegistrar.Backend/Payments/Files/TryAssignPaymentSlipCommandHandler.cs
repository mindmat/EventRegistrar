using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Files
{
    public class TryAssignPaymentSlipCommandHandler : IRequestHandler<TryAssignPaymentSlipCommand>
    {
        private readonly IRepository<ReceivedPayment> _payments;

        public TryAssignPaymentSlipCommandHandler(IRepository<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<Unit> Handle(TryAssignPaymentSlipCommand command, CancellationToken cancellationToken)
        {
            var payment = await _payments.Where(pmt => pmt.PaymentFile.EventId == command.EventId
                                                    && pmt.Reference == command.Reference)
                                         .ToListAsync(cancellationToken);

            if (payment.Count == 1)
            {
                payment[0].PaymentSlipId = command.PaymentSlipId;
            }

            return Unit.Value;
        }
    }
}