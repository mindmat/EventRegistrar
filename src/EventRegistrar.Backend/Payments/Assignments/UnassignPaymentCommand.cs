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

public class UnassignPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                           IEventBus eventBus,
                                           IDateTimeProvider dateTimeProvider,
                                           ChangeTrigger changeTrigger)
    : IRequestHandler<UnassignPaymentCommand>
{
    public async Task Handle(UnassignPaymentCommand command, CancellationToken cancellationToken)
    {
        var existingAssignment = await assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId
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
                                    Created = dateTimeProvider.Now
                                };
        existingAssignment.PaymentAssignmentId_Counter = counterAssignment.Id;
        await assignments.InsertOrUpdateEntity(counterAssignment, cancellationToken);

        if (existingAssignment.IncomingPaymentId != null)
        {
            eventBus.Publish(new IncomingPaymentUnassigned
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
            eventBus.Publish(new OutgoingPaymentUnassigned
                             {
                                 EventId = command.EventId,
                                 PaymentAssignmentId = command.PaymentAssignmentId,
                                 PaymentAssignmentId_Counter = counterAssignment.Id,
                                 OutgoingPaymentId = existingAssignment.OutgoingPaymentId!.Value,
                                 RegistrationId = existingAssignment.RegistrationId
                             });
        }

        changeTrigger.TriggerUpdate<RegistrationCalculator>(existingAssignment.RegistrationId, command.EventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);
    }
}