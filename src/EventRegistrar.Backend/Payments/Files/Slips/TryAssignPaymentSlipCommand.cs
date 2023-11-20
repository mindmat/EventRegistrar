namespace EventRegistrar.Backend.Payments.Files.Slips;

public class TryAssignPaymentSlipCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentSlipId { get; set; }
    public string? Reference { get; set; }
}

public class TryAssignPaymentSlipCommandHandler(IRepository<IncomingPayment> payments) : IRequestHandler<TryAssignPaymentSlipCommand>
{
    public async Task Handle(TryAssignPaymentSlipCommand command, CancellationToken cancellationToken)
    {
        var payment = await payments.Where(pmt => pmt.Payment!.EventId == command.EventId
                                               && pmt.Payment.InstructionIdentification == command.Reference)
                                    .ToListAsync(cancellationToken);

        if (payment.Count == 1)
        {
            payment[0].PaymentSlipId = command.PaymentSlipId;
        }
    }
}