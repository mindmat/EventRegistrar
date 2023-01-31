using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfOutgoingPaymentIsSettledCommand : IRequest
{
    public Guid OutgoingPaymentId { get; set; }
}

public class CheckIfOutgoingPaymentIsSettledCommandHandler : AsyncRequestHandler<CheckIfOutgoingPaymentIsSettledCommand>
{
    private readonly IRepository<OutgoingPayment> _outgoingPayments;

    public CheckIfOutgoingPaymentIsSettledCommandHandler(IRepository<OutgoingPayment> outgoingPayments)
    {
        _outgoingPayments = outgoingPayments;
    }

    protected override async Task Handle(CheckIfOutgoingPaymentIsSettledCommand command, CancellationToken cancellationToken)
    {
        var outgoingPayment = await _outgoingPayments.AsTracking()
                                                     .Where(pmt => pmt.Id == command.OutgoingPaymentId)
                                                     .Include(pmt => pmt.Payment)
                                                     .Include(pmt => pmt.Assignments)
                                                     .FirstAsync(cancellationToken);
        var balance = -outgoingPayment.Payment!.Amount
                    - outgoingPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);
        //+ outgoingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        outgoingPayment.Payment.Settled = balance == 0m;
    }
}

public class CheckIfOutgoingPaymentIsSettledAfterRepaymentAssignment : IEventToCommandTranslation<RepaymentAssigned>
{
    public IEnumerable<IRequest> Translate(RepaymentAssigned e)
    {
        yield return new CheckIfOutgoingPaymentIsSettledCommand { OutgoingPaymentId = e.OutgoingPaymentId };
    }
}