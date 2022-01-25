using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments;

internal class IgnorePaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

internal class IgnorePaymentCommandHandler : IRequestHandler<IgnorePaymentCommand>
{
    private readonly IRepository<BankAccountBooking> _payments;

    public IgnorePaymentCommandHandler(IRepository<BankAccountBooking> payments)
    {
        _payments = payments;
    }

    public async Task<Unit> Handle(IgnorePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _payments.FirstAsync(pmt => pmt.Id == request.PaymentId
                                                     && pmt.BankAccountStatementsFile.EventId == request.EventId);
        if (payment.Ignore)
        {
            return Unit.Value;
        }

        payment.Ignore = true;

        return Unit.Value;
    }
}