using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class UnassignIncomingPaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
}

public class UnassignIncomingPaymentCommandHandler : IRequestHandler<UnassignIncomingPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;

    public UnassignIncomingPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                                 IEventBus eventBus)
    {
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UnassignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var existingAssignment = await _assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId
                                                                   && ass.IncomingPayment != null, cancellationToken);
        if (existingAssignment.PaymentAssignmentId_Counter != null)
        {
            throw new ArgumentException($"Assignment {existingAssignment.Id} already has a counter assignment: {existingAssignment.PaymentAssignmentId_Counter}");
        }

        var counterAssignment = new PaymentAssignment
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = existingAssignment.RegistrationId,
                                    IncomingPaymentId = existingAssignment.IncomingPaymentId,
                                    PaymentAssignmentId_Counter = existingAssignment.Id,
                                    Amount = -existingAssignment.Amount,
                                    Created = DateTime.UtcNow
                                };
        existingAssignment.PaymentAssignmentId_Counter = counterAssignment.Id;
        await _assignments.InsertOrUpdateEntity(counterAssignment, cancellationToken);

        _eventBus.Publish(new IncomingPaymentUnassigned
                          {
                              EventId = command.EventId,
                              PaymentAssignmentId = command.PaymentAssignmentId,
                              PaymentAssignmentId_Counter = counterAssignment.Id,
                              IncomingPaymentId = existingAssignment.IncomingPaymentId,
                              RegistrationId = existingAssignment.RegistrationId
                          });
        return Unit.Value;
    }
}