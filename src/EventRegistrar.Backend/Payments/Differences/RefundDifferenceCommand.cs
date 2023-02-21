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

public class RefundDifferenceCommandHandler : AsyncRequestHandler<RefundDifferenceCommand>
{
    private readonly CommandQueue _commandQueue;
    private readonly IQueryable<Registration> _registrations;
    private readonly IRepository<PayoutRequest> _payoutRequests;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEventBus _eventBus;

    public RefundDifferenceCommandHandler(CommandQueue commandQueue,
                                          IQueryable<Registration> registrations,
                                          IRepository<PayoutRequest> payoutRequests,
                                          IDateTimeProvider dateTimeProvider,
                                          IEventBus eventBus)
    {
        _commandQueue = commandQueue;
        _registrations = registrations;
        _payoutRequests = payoutRequests;
        _dateTimeProvider = dateTimeProvider;
        _eventBus = eventBus;
    }

    protected override async Task Handle(RefundDifferenceCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                               .Include(reg => reg.PaymentAssignments!)
                                               .ThenInclude(pas => pas.IncomingPayment)
                                               .FirstAsync(cancellationToken);
        var data = new TooMuchPaidMailData
                   {
                       Price = registration.Price_AdmittedAndReduced,
                       AmountPaid = registration.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
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
                                Created = _dateTimeProvider.Now,
                                IbanProposed = registration.PaymentAssignments!
                                                           .Select(pas => pas.IncomingPayment?.DebitorIban)
                                                           .FirstOrDefault(iban => iban != null)
                            };
        await _payoutRequests.InsertOrUpdateEntity(payoutRequest, cancellationToken);

        var sendMailCommand = new ComposeAndSendAutoMailCommand
                              {
                                  EventId = command.EventId,
                                  MailType = MailType.TooMuchPaid,
                                  RegistrationId = command.RegistrationId,
                                  Data = data
                              };
        _commandQueue.EnqueueCommand(sendMailCommand);

        _eventBus.Publish(new QueryChanged
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