using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

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

    public UnassignPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                         IEventBus eventBus)
    {
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UnassignPaymentCommand command, CancellationToken cancellationToken)
    {
        var assignment = await _assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId, cancellationToken);
        if (assignment.PaymentAssignmentId_Counter != null)
        {
            throw new ArgumentException(
                $"Assignment {assignment.Id} already has a counter assignment: {assignment.PaymentAssignmentId_Counter}");
        }

        var counterAssignment = new PaymentAssignment
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = assignment.RegistrationId,
                                    ReceivedPaymentId = assignment.ReceivedPaymentId,
                                    PaymentAssignmentId_Counter = assignment.Id,
                                    Amount = -assignment.Amount,
                                    Created = DateTime.UtcNow
                                };
        assignment.PaymentAssignmentId_Counter = counterAssignment.Id;
        await _assignments.InsertOrUpdateEntity(counterAssignment, cancellationToken);
        _eventBus.Publish(new PaymentUnassigned
                          {
                              EventId = command.EventId,
                              PaymentAssignmentId = command.PaymentAssignmentId,
                              PaymentAssignmentId_Counter = counterAssignment.Id,
                              PaymentId = assignment.ReceivedPaymentId,
                              RegistrationId = assignment.RegistrationId
                          });
        return Unit.Value;
    }
}