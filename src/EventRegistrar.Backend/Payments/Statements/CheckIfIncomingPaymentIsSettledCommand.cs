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
        var incomingPayment = await _incomingPayments.AsTracking()
                                                     .Where(pmt => pmt.Id == command.IncomingPaymentId)
                                                     .Include(pmt => pmt.Payment)
                                                     .Include(pmt => pmt.Assignments)
                                                     .FirstAsync(cancellationToken);
        var balance = incomingPayment.Payment!.Amount
                    - incomingPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);
        //+ incomingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        incomingPayment.Payment.Settled = balance == 0m;
        return Unit.Value;
    }
}