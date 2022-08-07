using EventRegistrar.Backend.Infrastructure.DataAccess;

using MediatR;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class TryAssignPaymentSlipCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentSlipId { get; set; }
    public string Reference { get; set; }
}

public class TryAssignPaymentSlipCommandHandler : IRequestHandler<TryAssignPaymentSlipCommand>
{
    private readonly IRepository<IncomingPayment> _payments;

    public TryAssignPaymentSlipCommandHandler(IRepository<IncomingPayment> payments)
    {
        _payments = payments;
    }

    public async Task<Unit> Handle(TryAssignPaymentSlipCommand command, CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Payment!.PaymentsFile!.EventId == command.EventId
                                                && pmt.Payment.InstructionIdentification == command.Reference)
                                     .ToListAsync(cancellationToken);

        if (payment.Count == 1)
        {
            payment[0].PaymentSlipId = command.PaymentSlipId;
        }

        return Unit.Value;
    }
}