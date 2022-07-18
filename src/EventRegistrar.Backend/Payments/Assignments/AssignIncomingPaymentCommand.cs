using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.IndividualReductions;

using MediatR;

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

public class AssignIncomingPaymentCommandHandler : IRequestHandler<AssignIncomingPaymentCommand>
{
    private readonly IRepository<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;
    private readonly IRepository<IndividualReduction> _individualReductions;
    private readonly IQueryable<IncomingPayment> _incomingPayments;
    private readonly IQueryable<Registration> _registrations;
    private readonly AuthenticatedUserId _userId;

    public AssignIncomingPaymentCommandHandler(IQueryable<Registration> registrations,
                                               IQueryable<IncomingPayment> incomingPayments,
                                               IRepository<PaymentAssignment> assignments,
                                               IRepository<IndividualReduction> individualReductions,
                                               IEventBus eventBus,
                                               AuthenticatedUserId userId)
    {
        _registrations = registrations;
        _incomingPayments = incomingPayments;
        _assignments = assignments;
        _individualReductions = individualReductions;
        _eventBus = eventBus;
        _userId = userId;
    }

    public async Task<Unit> Handle(AssignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                               .Include(reg => reg.PaymentAssignments)
                                               .Include(reg => reg.IndividualReductions)
                                               .FirstAsync(cancellationToken);
        var incomingPayment = await _incomingPayments.FirstAsync(pmt => pmt.Id == command.PaymentIncomingId, cancellationToken);

        var assignment = new PaymentAssignment
                         {
                             Id = Guid.NewGuid(),
                             RegistrationId = registration.Id,
                             IncomingPaymentId = incomingPayment.Id,
                             Amount = command.Amount,
                             Created = DateTime.UtcNow
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        if (command.AcceptDifference)
        {
            var difference = (registration.Price ?? 0m)
                           - registration.PaymentAssignments!.Sum(pmt => pmt.PayoutRequestId == null ? pmt.Amount : -pmt.Amount);
            await _individualReductions.InsertOrUpdateEntity(new IndividualReduction
                                                             {
                                                                 Id = Guid.NewGuid(),
                                                                 RegistrationId = registration.Id,
                                                                 Amount = difference,
                                                                 Reason = command.AcceptDifferenceReason,
                                                                 UserId = _userId.UserId ?? Guid.Empty
                                                             }, cancellationToken);

            _eventBus.Publish(new IndividualReductionAdded
                              {
                                  RegistrationId = registration.Id,
                                  Amount = difference,
                                  Reason = command.AcceptDifferenceReason
                              });
        }

        _eventBus.Publish(new IncomingPaymentAssigned
                          {
                              PaymentAssignmentId = assignment.Id,
                              Amount = assignment.Amount,
                              RegistrationId = registration.Id,
                              IncomingPaymentId = assignment.IncomingPaymentId
                          });

        return Unit.Value;
    }
}