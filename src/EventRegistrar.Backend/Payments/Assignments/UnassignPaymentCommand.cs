using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Assignments;

public class UnassignPaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
}

public class UnassignPaymentCommandHandler : IRequestHandler<UnassignPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UnassignPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                         IEventBus eventBus,
                                         IDateTimeProvider dateTimeProvider)
    {
        _assignments = assignments;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UnassignPaymentCommand command, CancellationToken cancellationToken)
    {
        var existingAssignment = await _assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId
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

        return Unit.Value;
    }
}