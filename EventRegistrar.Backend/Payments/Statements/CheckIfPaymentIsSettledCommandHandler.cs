using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class CheckIfPaymentIsSettledCommandHandler : IRequestHandler<CheckIfPaymentIsSettledCommand>
    {
        private readonly IRepository<ReceivedPayment> _payments;

        public CheckIfPaymentIsSettledCommandHandler(IRepository<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<Unit> Handle(CheckIfPaymentIsSettledCommand command, CancellationToken cancellationToken)
        {
            var payment = await _payments.Where(pmt => pmt.Id == command.PaymentId)
                                         .Include(pmt => pmt.Assignments)
                                         .FirstAsync(cancellationToken);
            payment.Settled = payment.Amount == payment.Assignments.Sum(ass => ass.Amount);
            return Unit.Value;
        }
    }
}