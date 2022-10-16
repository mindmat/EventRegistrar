using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

using MediatR;


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

public class AssignOutgoingPaymentCommandHandler : IRequestHandler<AssignOutgoingPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<PayoutRequest> _payoutRequests;
    private readonly IQueryable<OutgoingPayment> _outgoingPayments;
    private readonly IQueryable<Registration> _registrations;

    public AssignOutgoingPaymentCommandHandler(IQueryable<PayoutRequest> payoutRequests,
                                               IQueryable<OutgoingPayment> outgoingPayments,
                                               IQueryable<Registration> registrations,
                                               IRepository<PaymentAssignment> assignments,
                                               IEventBus eventBus,
                                               IDateTimeProvider dateTimeProvider)
    {
        _payoutRequests = payoutRequests;
        _outgoingPayments = outgoingPayments;
        _registrations = registrations;
        _assignments = assignments;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(AssignOutgoingPaymentCommand command, CancellationToken cancellationToken)
    {
        Guid registrationId;
        if (command.PayoutRequestId != null)
        {
            registrationId = await _payoutRequests.Where(por => por.Id == command.PayoutRequestId
                                                             && por.Registration!.EventId == command.EventId)
                                                  .Select(por => por.RegistrationId)
                                                  .FirstAsync(cancellationToken);
        }
        else if (command.RegistrationId != null)
        {
            // only to validate registration id vs event id
            registrationId = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                 .Select(reg => reg.Id)
                                                 .FirstAsync(cancellationToken);
        }
        else
        {
            throw new ArgumentNullException("Either RegistrationId or PayoutRequestId have to be set", (Exception?)null);
        }

        var outgoingPayment = await _outgoingPayments.FirstAsync(pmt => pmt.Id == command.OutgoingPaymentId, cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = registrationId,
                             PayoutRequestId = command.PayoutRequestId,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = command.Amount,
                             Created = _dateTimeProvider.Now
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        _eventBus.Publish(new OutgoingPaymentAssigned
                          {
                              PaymentAssignmentId = assignment.Id,
                              RegistrationId = assignment.RegistrationId,
                              PayoutRequestId = assignment.PayoutRequestId,
                              OutgoingPaymentId = outgoingPayment.Id,
                              Amount = assignment.Amount
                          });

        return Unit.Value;
    }
}