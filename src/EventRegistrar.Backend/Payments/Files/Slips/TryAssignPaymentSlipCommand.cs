using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class TryAssignPaymentSlipCommand : IRequest, IQueueBoundMessage
{
    public Guid EventId { get; set; }
    public Guid PaymentSlipId { get; set; }
    public string QueueName => "TryAssignPaymentSlipQueue";
    public string Reference { get; set; }
}

public class TryAssignPaymentSlipCommandHandler : IRequestHandler<TryAssignPaymentSlipCommand>
{
    private readonly IRepository<ReceivedPayment> _payments;

    public TryAssignPaymentSlipCommandHandler(IRepository<ReceivedPayment> payments)
    {
        _payments = payments;
    }

    public async Task<Unit> Handle(TryAssignPaymentSlipCommand command, CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.PaymentFile!.EventId == command.EventId
                                                && pmt.InstructionIdentification == command.Reference)
                                     .ToListAsync(cancellationToken);

        if (payment.Count == 1)
        {
            payment[0].PaymentSlipId = command.PaymentSlipId;
        }

        return Unit.Value;
    }
}