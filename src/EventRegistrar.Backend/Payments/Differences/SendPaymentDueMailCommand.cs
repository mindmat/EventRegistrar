using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Differences;

public class SendPaymentDueMailCommand : IRequest, IEventBoundRequest
{
    public Guid RegistrationId { get; set; }
    public Guid EventId { get; set; }
}

public class SendPaymentDueMailCommandHandler(CommandQueue commandQueue,
                                              IQueryable<Registration> registrations)
    : IRequestHandler<SendPaymentDueMailCommand>
{
    public async Task Handle(SendPaymentDueMailCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.Where(reg => reg.Id == command.RegistrationId)
                                              .Include(reg => reg.PaymentAssignments)
                                              .FirstAsync(cancellationToken);
        var data = new PaymentDueMailData
                   {
                       Price = registration.Price_AdmittedAndReduced,
                       AmountPaid = registration.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
                   };
        if (data.Price <= data.AmountPaid)
        {
            throw new Exception("No money owed");
        }

        var sendMailCommand = new ComposeAndSendAutoMailCommand
                              {
                                  EventId = command.EventId,
                                  MailType = MailType.MoneyOwed,
                                  RegistrationId = command.RegistrationId,
                                  Data = data
                              };
        commandQueue.EnqueueCommand(sendMailCommand);
    }
}

public class PaymentDueMailData
{
    public decimal Price { get; set; }
    public decimal AmountPaid { get; set; }
}