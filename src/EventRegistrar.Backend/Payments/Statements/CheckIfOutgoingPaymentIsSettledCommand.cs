using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfOutgoingPaymentIsSettledCommand : IRequest
{
    public Guid OutgoingPaymentId { get; set; }
}

public class CheckIfOutgoingPaymentIsSettledCommandHandler(IRepository<OutgoingPayment> outgoingPayments) : IRequestHandler<CheckIfOutgoingPaymentIsSettledCommand>
{
    public async Task Handle(CheckIfOutgoingPaymentIsSettledCommand command, CancellationToken cancellationToken)
    {
        var outgoingPayment = await outgoingPayments.AsTracking()
                                                    .Where(pmt => pmt.Id == command.OutgoingPaymentId)
                                                    .Include(pmt => pmt.Payment)
                                                    .Include(pmt => pmt.Assignments)
                                                    .FirstAsync(cancellationToken);
        var balance = outgoingPayment.Payment!.Amount
                    - outgoingPayment.Assignments!.Sum(asn => asn.Amount);
        //+ outgoingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        outgoingPayment.Payment.Settled = balance == 0m;
    }
}

public class CheckIfOutgoingPaymentIsSettled : IEventToCommandTranslation<RepaymentAssigned>,
                                               IEventToCommandTranslation<OutgoingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(RepaymentAssigned e)
    {
        yield return new CheckIfOutgoingPaymentIsSettledCommand { OutgoingPaymentId = e.OutgoingPaymentId };
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        yield return new CheckIfOutgoingPaymentIsSettledCommand { OutgoingPaymentId = e.OutgoingPaymentId };
    }
}