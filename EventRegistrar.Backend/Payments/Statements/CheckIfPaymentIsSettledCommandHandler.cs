using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files.Camt;
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
                                         .Include(pmt => pmt.RepaymentAssignments)
                                         .FirstAsync(cancellationToken);
            var balance = (payment.CreditDebitType == CreditDebit.DBIT ? -payment.Amount : payment.Amount)
                          - payment.Assignments.Sum(ass => ass.Amount)
                          + payment.RepaymentAssignments.Sum(ass => ass.Amount);
            payment.Settled = balance == 0m;
            return Unit.Value;
        }
    }
}