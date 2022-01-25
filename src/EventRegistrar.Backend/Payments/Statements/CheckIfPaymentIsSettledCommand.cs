using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfPaymentIsSettledCommand : IRequest
{
    public Guid PaymentId { get; set; }
}

public class CheckIfPaymentIsSettledCommandHandler : IRequestHandler<CheckIfPaymentIsSettledCommand>
{
    private readonly IRepository<BankAccountBooking> _payments;

    public CheckIfPaymentIsSettledCommandHandler(IRepository<BankAccountBooking> payments)
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
                    - payment.Assignments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
                    + payment.RepaymentAssignments.Sum(asn => asn.Amount);
        payment.Settled = balance == 0m;
        return Unit.Value;
    }
}