using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.IndividualReductions;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignIncomingPaymentCommand : IRequest, IEventBoundRequest
{
    public bool AcceptDifference { get; set; }
    public string? AcceptDifferenceReason { get; set; }
    public decimal Amount { get; set; }
    public Guid EventId { get; set; }
    public Guid PaymentIncomingId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class AssignIncomingPaymentCommandHandler(IQueryable<Registration> registrations,
                                                 IQueryable<IncomingPayment> incomingPayments,
                                                 IRepository<PaymentAssignment> assignments,
                                                 IRepository<IndividualReduction> individualReductions,
                                                 IEventBus eventBus,
                                                 AuthenticatedUserId userId,
                                                 IDateTimeProvider dateTimeProvider,
                                                 ChangeTrigger changeTrigger)
    : IRequestHandler<AssignIncomingPaymentCommand>
{
    public async Task Handle(AssignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.Where(reg => reg.Id == command.RegistrationId
                                                         && reg.EventId == command.EventId)
                                              .Include(reg => reg.PaymentAssignments)
                                              .FirstAsync(cancellationToken);
        var incomingPayment = await incomingPayments.FirstAsync(pmt => pmt.Id == command.PaymentIncomingId, cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = registration.Id,
                             IncomingPaymentId = incomingPayment.Id,
                             Amount = command.Amount,
                             Created = dateTimeProvider.Now
                         };
        await assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        if (command.AcceptDifference)
        {
            var difference = registration.Price_AdmittedAndReduced
                           - registration.PaymentAssignments!.Sum(pmt => pmt.OutgoingPayment == null
                                                                             ? pmt.Amount
                                                                             : -pmt.Amount);
            await individualReductions.InsertOrUpdateEntity(new IndividualReduction
                                                            {
                                                                Id = Guid.NewGuid(),
                                                                RegistrationId = registration.Id,
                                                                Amount = difference,
                                                                Reason = command.AcceptDifferenceReason,
                                                                UserId = userId.UserId ?? Guid.Empty
                                                            }, cancellationToken);

            eventBus.Publish(new IndividualReductionAdded
                             {
                                 RegistrationId = registration.Id,
                                 Amount = difference,
                                 Reason = command.AcceptDifferenceReason
                             });
        }

        eventBus.Publish(new IncomingPaymentAssigned
                         {
                             PaymentAssignmentId = assignment.Id,
                             Amount = assignment.Amount,
                             RegistrationId = registration.Id,
                             IncomingPaymentId = incomingPayment.Id
                         });

        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);
        changeTrigger.QueryChanged<PaymentsByDayQuery>(registration.EventId);
        changeTrigger.QueryChanged<PaymentAssignmentsQuery>(registration.EventId, incomingPayment.Id);
    }
}