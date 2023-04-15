using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CreateAndAssignIncomingPaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? DebitorName { get; set; }
    public string? DebitorIban { get; set; }
    public DateTime? BookingDate { get; set; }
    public string? Message { get; set; }
}

public class AddAndAssignManualIncomingPaymentCommandHandler : AsyncRequestHandler<CreateAndAssignIncomingPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IQueryable<Registration> _registrations;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;

    public AddAndAssignManualIncomingPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                                           IQueryable<Registration> registrations,
                                                           IDateTimeProvider dateTimeProvider,
                                                           ReadModelUpdater readModelUpdater)
    {
        _assignments = assignments;
        _registrations = registrations;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(CreateAndAssignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId
                                                          && reg.EventId == command.EventId)
                                               .Include(reg => reg.PaymentAssignments)
                                               .FirstAsync(cancellationToken);

        _assignments.InsertObjectTree(new PaymentAssignment
                                      {
                                          Id = Guid.NewGuid(),
                                          Amount = command.Amount,
                                          RegistrationId = registration.Id,
                                          Created = _dateTimeProvider.Now,
                                          IncomingPayment = new IncomingPayment
                                                            {
                                                                DebitorName = command.DebitorName,
                                                                DebitorIban = command.DebitorIban,
                                                                Payment = new Payment
                                                                          {
                                                                              Id = command.PaymentId,
                                                                              EventId = command.EventId,
                                                                              Amount = command.Amount,
                                                                              BookingDate = command.BookingDate ?? _dateTimeProvider.Now.Date,
                                                                              Message = command.Message,
                                                                              Type = PaymentType.Incoming
                                                                          }
                                                            }
                                      });
        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);
    }
}