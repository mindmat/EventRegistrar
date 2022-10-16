using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class AssignRepaymentCommand : IRequest, IEventBoundRequest
{
    public decimal Amount { get; set; }
    public Guid EventId { get; set; }
    public Guid IncomingPaymentId { get; set; }
    public Guid OutgoingPaymentId { get; set; }
}

public class AssignRepaymentCommandHandler : IRequestHandler<AssignRepaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<IncomingPayment> _incomingPayments;
    private readonly IQueryable<OutgoingPayment> _outgoingPayments;

    public AssignRepaymentCommandHandler(IQueryable<IncomingPayment> incomingIncomingPayments,
                                         IQueryable<OutgoingPayment> outgoingPayments,
                                         IRepository<PaymentAssignment> assignments,
                                         IEventBus eventBus,
                                         IDateTimeProvider dateTimeProvider)
    {
        _incomingPayments = incomingIncomingPayments;
        _assignments = assignments;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
        _outgoingPayments = outgoingPayments;
    }

    public async Task<Unit> Handle(AssignRepaymentCommand command, CancellationToken cancellationToken)
    {
        var incomingPayment = await _incomingPayments.FirstAsync(pmt => pmt.Id == command.IncomingPaymentId
                                                                     && pmt.Payment!.PaymentsFile!.EventId == command.EventId,
                                                                 cancellationToken);
        var outgoingPayment = await _outgoingPayments.FirstAsync(pmt => pmt.Id == command.OutgoingPaymentId
                                                                     && pmt.Payment!.PaymentsFile!.EventId == command.EventId,
                                                                 cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             IncomingPaymentId = incomingPayment.Id,
                             OutgoingPaymentId = outgoingPayment.Id,
                             Amount = command.Amount,
                             Created = _dateTimeProvider.Now
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        _eventBus.Publish(new RepaymentAssigned
                          {
                              PaymentAssignmentId = assignment.Id,
                              Amount = assignment.Amount,
                              EventId = command.EventId,
                              IncomingPaymentId = incomingPayment.Id,
                              OutgoingPaymentId = outgoingPayment.Id
                          });

        return Unit.Value;
    }
}