using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Payments.Assignments;

public class UnassignPaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
}

public class UnassignPaymentCommandHandler : AsyncRequestHandler<UnassignPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;

    public UnassignPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                         IEventBus eventBus,
                                         IDateTimeProvider dateTimeProvider,
                                         ReadModelUpdater readModelUpdater)
    {
        _assignments = assignments;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(UnassignPaymentCommand command, CancellationToken cancellationToken)
    {
        var existingAssignment = await _assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId
                                                                   && ass.Registration!.EventId == command.EventId
                                                                   && (ass.IncomingPaymentId != null || ass.OutgoingPaymentId != null), cancellationToken);
        if (existingAssignment.PaymentAssignmentId_Counter != null)
        {
            throw new ArgumentException($"Assignment {existingAssignment.Id} already has a counter assignment: {existingAssignment.PaymentAssignmentId_Counter}");
        }

        var counterAssignment = new PaymentAssignment
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = existingAssignment.RegistrationId,
                                    IncomingPaymentId = existingAssignment.IncomingPaymentId,
                                    OutgoingPaymentId = existingAssignment.OutgoingPaymentId,
                                    PaymentAssignmentId_Counter = existingAssignment.Id,
                                    Amount = -existingAssignment.Amount,
                                    Created = _dateTimeProvider.Now
                                };
        existingAssignment.PaymentAssignmentId_Counter = counterAssignment.Id;
        await _assignments.InsertOrUpdateEntity(counterAssignment, cancellationToken);

        if (existingAssignment.IncomingPaymentId != null)
        {
            _eventBus.Publish(new IncomingPaymentUnassigned
                              {
                                  EventId = command.EventId,
                                  PaymentAssignmentId = command.PaymentAssignmentId,
                                  PaymentAssignmentId_Counter = counterAssignment.Id,
                                  IncomingPaymentId = existingAssignment.IncomingPaymentId!.Value,
                                  RegistrationId = existingAssignment.RegistrationId
                              });
        }
        else if (existingAssignment.OutgoingPaymentId != null)
        {
            _eventBus.Publish(new OutgoingPaymentUnassigned
                              {
                                  EventId = command.EventId,
                                  PaymentAssignmentId = command.PaymentAssignmentId,
                                  PaymentAssignmentId_Counter = counterAssignment.Id,
                                  OutgoingPaymentId = existingAssignment.OutgoingPaymentId!.Value,
                                  RegistrationId = existingAssignment.RegistrationId
                              });
        }

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(existingAssignment.RegistrationId, command.EventId);
        _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);
    }
}