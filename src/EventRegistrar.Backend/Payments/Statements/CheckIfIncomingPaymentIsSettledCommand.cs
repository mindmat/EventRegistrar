﻿using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfIncomingPaymentIsSettledCommand : IRequest
{
    public Guid IncomingPaymentId { get; set; }
}

public class CheckIfIncomingPaymentIsSettledCommandHandler : AsyncRequestHandler<CheckIfIncomingPaymentIsSettledCommand>
{
    private readonly IRepository<IncomingPayment> _incomingPayments;

    public CheckIfIncomingPaymentIsSettledCommandHandler(IRepository<IncomingPayment> incomingPayments)
    {
        _incomingPayments = incomingPayments;
    }

    protected override async Task Handle(CheckIfIncomingPaymentIsSettledCommand command, CancellationToken cancellationToken)
    {
        var incomingPayment = await _incomingPayments.AsTracking()
                                                     .Where(pmt => pmt.Id == command.IncomingPaymentId)
                                                     .Include(pmt => pmt.Payment)
                                                     .Include(pmt => pmt.Assignments)
                                                     .FirstAsync(cancellationToken);
        var balance = incomingPayment.Payment!.Amount
                    - incomingPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                  ? asn.Amount
                                                                  : -asn.Amount);
        //+ incomingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        incomingPayment.Payment.Settled = balance == 0m;
    }
}

public class CheckIfIncomingPaymentIsSettledAfterRepaymentAssignment : IEventToCommandTranslation<RepaymentAssigned>
{
    public IEnumerable<IRequest> Translate(RepaymentAssigned e)
    {
        yield return new CheckIfIncomingPaymentIsSettledCommand { IncomingPaymentId = e.IncomingPaymentId };
    }
}