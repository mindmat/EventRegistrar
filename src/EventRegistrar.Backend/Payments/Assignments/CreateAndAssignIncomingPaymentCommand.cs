using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

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

public class AddAndAssignManualIncomingPaymentCommandHandler(IRepository<PaymentAssignment> assignments,
                                                             IQueryable<Registration> registrations,
                                                             IDateTimeProvider dateTimeProvider,
                                                             ChangeTrigger changeTrigger,
                                                             IEventBus eventBus)
    : IRequestHandler<CreateAndAssignIncomingPaymentCommand>
{
    public async Task Handle(CreateAndAssignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.Where(reg => reg.Id == command.RegistrationId
                                                         && reg.EventId == command.EventId)
                                              .Include(reg => reg.PaymentAssignments)
                                              .FirstAsync(cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             Amount = command.Amount,
                             RegistrationId = registration.Id,
                             Created = dateTimeProvider.Now,
                             IncomingPayment = new IncomingPayment
                                               {
                                                   Id = Guid.NewGuid(),
                                                   DebitorName = command.DebitorName,
                                                   DebitorIban = command.DebitorIban,
                                                   Payment = new Payment
                                                             {
                                                                 Id = command.PaymentId,
                                                                 EventId = command.EventId,
                                                                 Amount = command.Amount,
                                                                 BookingDate = command.BookingDate ?? dateTimeProvider.Now.Date,
                                                                 Message = command.Message,
                                                                 Type = PaymentType.Incoming
                                                             }
                                               }
                         };
        assignments.InsertObjectTree(assignment);

        eventBus.Publish(new IncomingPaymentAssigned
                         {
                             PaymentAssignmentId = assignment.Id,
                             Amount = assignment.Amount,
                             RegistrationId = registration.Id,
                             IncomingPaymentId = assignment.IncomingPayment.Id
                         });

        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);
    }
}