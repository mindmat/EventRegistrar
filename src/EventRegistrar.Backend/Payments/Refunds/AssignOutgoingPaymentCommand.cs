using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;


namespace EventRegistrar.Backend.Payments.Refunds;

public class AssignOutgoingPaymentCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
    public Guid? PayoutRequestId { get; set; }
    public Guid? RegistrationId { get; set; }
    public decimal Amount { get; set; }

    public bool AcceptDifference { get; set; }
    public string? AcceptDifferenceReason { get; set; }
}

public class AssignOutgoingPaymentCommandHandler(IQueryable<PayoutRequest> payoutRequests,
                                                 IQueryable<OutgoingPayment> outgoingPayments,
                                                 IQueryable<Registration> registrations,
                                                 IRepository<PaymentAssignment> assignments,
                                                 IEventBus eventBus,
                                                 IDateTimeProvider dateTimeProvider)
    : IRequestHandler<AssignOutgoingPaymentCommand>
{
    public async Task Handle(AssignOutgoingPaymentCommand command, CancellationToken cancellationToken)
    {
        Guid registrationId;
        if (command.PayoutRequestId != null)
        {
            registrationId = await payoutRequests.Where(por => por.Id == command.PayoutRequestId
                                                            && por.Registration!.EventId == command.EventId)
                                                 .Select(por => por.RegistrationId)
                                                 .FirstAsync(cancellationToken);
        }
        else if (command.RegistrationId != null)
        {
            // only to validate registration id vs event id
            registrationId = await registrations.Where(reg => reg.Id == command.RegistrationId
                                                           && reg.EventId == command.EventId)
                                                .Select(reg => reg.Id)
                                                .FirstAsync(cancellationToken);
        }
        else
        {
            throw new ArgumentNullException("Either RegistrationId or PayoutRequestId have to be set", (Exception?)null);
        }

        var outgoingPayment = await outgoingPayments.FirstAsync(pmt => pmt.Id == command.OutgoingPaymentId, cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = registrationId,
                             PayoutRequestId = command.PayoutRequestId,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = command.Amount,
                             Created = dateTimeProvider.Now
                         };
        await assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        eventBus.Publish(new OutgoingPaymentAssigned
                         {
                             PaymentAssignmentId = assignment.Id,
                             RegistrationId = assignment.RegistrationId,
                             PayoutRequestId = assignment.PayoutRequestId,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = assignment.Amount
                         });
    }
}