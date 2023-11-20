namespace EventRegistrar.Backend.Payments.Refunds;

public class CheckIfPayoutIsConfirmedCommand : IRequest
{
    public Guid PayoutRequestId { get; set; }
}

public class CheckIfPayoutIsConfirmedCommandHandler(IRepository<PayoutRequest> payoutRequests) : IRequestHandler<CheckIfPayoutIsConfirmedCommand>
{
    public async Task Handle(CheckIfPayoutIsConfirmedCommand command, CancellationToken cancellationToken)
    {
        var payoutRequest = await payoutRequests.Where(pmt => pmt.Id == command.PayoutRequestId)
                                                .Include(pmt => pmt.Assignments)
                                                .FirstAsync(cancellationToken);
        var balance = payoutRequest.Amount
                    - payoutRequest.Assignments!.Sum(asn => asn.Amount);
        payoutRequest.State = balance == 0m
                                  ? PayoutState.Confirmed
                                  : PayoutState.Requested;
    }
}