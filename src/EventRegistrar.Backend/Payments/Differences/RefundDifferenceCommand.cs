using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Differences;

public class RefundDifferenceCommand : IRequest, IEventBoundRequest
{
    public Guid RegistrationId { get; set; }
    public Guid EventId { get; set; }
    public string? Reason { get; set; }
}

public class RefundDifferenceCommandHandler(CommandQueue commandQueue,
                                            IQueryable<Registration> registrations,
                                            IRepository<PayoutRequest> payoutRequests,
                                            IDateTimeProvider dateTimeProvider,
                                            IEventBus eventBus)
    : IRequestHandler<RefundDifferenceCommand>
{
    public async Task Handle(RefundDifferenceCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.Where(reg => reg.Id == command.RegistrationId)
                                              .Include(reg => reg.PaymentAssignments!)
                                              .ThenInclude(pas => pas.IncomingPayment)
                                              .FirstAsync(cancellationToken);
        var data = new TooMuchPaidMailData
                   {
                       Price = registration.Price_AdmittedAndReduced,
                       AmountPaid = registration.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
                                                                                    ? asn.Amount
                                                                                    : -asn.Amount)
                   };
        data.RefundAmount = data.AmountPaid - data.Price;
        if (data.RefundAmount <= 0m)
        {
            throw new Exception("Not too much paid");
        }

        var payoutRequest = new PayoutRequest
                            {
                                RegistrationId = command.RegistrationId,
                                Amount = data.RefundAmount,
                                Reason = command.Reason ?? "Refund of difference",
                                State = PayoutState.Requested,
                                Created = dateTimeProvider.Now,
                                IbanProposed = registration.PaymentAssignments!
                                                           .Select(pas => pas.IncomingPayment?.DebitorIban)
                                                           .FirstOrDefault(iban => iban != null)
                            };
        await payoutRequests.InsertOrUpdateEntity(payoutRequest, cancellationToken);

        var sendMailCommand = new ComposeAndSendAutoMailCommand
                              {
                                  EventId = command.EventId,
                                  MailType = MailType.TooMuchPaid,
                                  RegistrationId = command.RegistrationId,
                                  Data = data
                              };
        commandQueue.EnqueueCommand(sendMailCommand);

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(DifferencesQuery)
                         });
    }
}

public class TooMuchPaidMailData
{
    public decimal Price { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RefundAmount { get; set; }
}