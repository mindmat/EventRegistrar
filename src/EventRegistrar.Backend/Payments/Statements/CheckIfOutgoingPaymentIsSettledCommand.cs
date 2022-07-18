using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfOutgoingPaymentIsSettledCommand : IRequest
{
    public Guid OutgoingPaymentId { get; set; }
}

public class CheckIfOutgoingPaymentIsSettledCommandHandler : IRequestHandler<CheckIfOutgoingPaymentIsSettledCommand>
{
    private readonly IRepository<OutgoingPayment> _outgoingPayments;

    public CheckIfOutgoingPaymentIsSettledCommandHandler(IRepository<OutgoingPayment> outgoingPayments)
    {
        _outgoingPayments = outgoingPayments;
    }

    public async Task<Unit> Handle(CheckIfOutgoingPaymentIsSettledCommand command, CancellationToken cancellationToken)
    {
        var outgoingPayment = await _outgoingPayments.Where(pmt => pmt.Id == command.OutgoingPaymentId)
                                                     .Include(pmt => pmt.Assignments)
                                                     .Include(pmt => pmt.RepaymentAssignments)
                                                     .FirstAsync(cancellationToken);
        var balance = -outgoingPayment.Amount
                    - outgoingPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
                    + outgoingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        outgoingPayment.Settled = balance == 0m;
        return Unit.Value;
    }
}