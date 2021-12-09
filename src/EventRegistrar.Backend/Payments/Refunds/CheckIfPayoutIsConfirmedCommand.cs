using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds;

public class CheckIfPayoutIsConfirmedCommand : IRequest
{
    public Guid PayoutRequestId { get; set; }
}

public class CheckIfPayoutIsConfirmedCommandHandler : IRequestHandler<CheckIfPayoutIsConfirmedCommand>
{
    private readonly IRepository<PayoutRequest> _payoutRequests;

    public CheckIfPayoutIsConfirmedCommandHandler(IRepository<PayoutRequest> payoutRequests)
    {
        _payoutRequests = payoutRequests;
    }

    public async Task<Unit> Handle(CheckIfPayoutIsConfirmedCommand command, CancellationToken cancellationToken)
    {
        var payoutRequest = await _payoutRequests.Where(pmt => pmt.Id == command.PayoutRequestId)
                                                 .Include(pmt => pmt.Assignments)
                                                 .FirstAsync(cancellationToken);
        var balance = payoutRequest.Amount
                    - payoutRequest.Assignments.Sum(asn => asn.Amount);
        payoutRequest.State = balance == 0m ? PayoutState.Confirmed : PayoutState.Requested;
        return Unit.Value;
    }
}