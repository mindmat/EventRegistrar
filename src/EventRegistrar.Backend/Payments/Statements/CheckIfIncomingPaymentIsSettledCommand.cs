using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfIncomingPaymentIsSettledCommand : IRequest
{
    public Guid IncomingPaymentId { get; set; }
}

public class CheckIfIncomingPaymentIsSettledCommandHandler : IRequestHandler<CheckIfIncomingPaymentIsSettledCommand>
{
    private readonly IRepository<IncomingPayment> _incomingPayments;

    public CheckIfIncomingPaymentIsSettledCommandHandler(IRepository<IncomingPayment> incomingPayments)
    {
        _incomingPayments = incomingPayments;
    }

    public async Task<Unit> Handle(CheckIfIncomingPaymentIsSettledCommand command, CancellationToken cancellationToken)
    {
        var incomingPayment = await _incomingPayments.Where(pmt => pmt.Id == command.IncomingPaymentId)
                                                     .Include(pmt => pmt.Assignments)
                                                     .Include(pmt => pmt.RepaymentAssignments)
                                                     .FirstAsync(cancellationToken);
        var balance = incomingPayment.Amount
                    - incomingPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
                    + incomingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        incomingPayment.Settled = balance == 0m;
        return Unit.Value;
    }
}