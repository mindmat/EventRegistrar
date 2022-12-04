using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.ReadModels;

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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;

    public AssignIncomingPaymentCommandHandler(IQueryable<Registration> registrations,
                                               IQueryable<IncomingPayment> incomingPayments,
                                               IRepository<PaymentAssignment> assignments,
                                               IRepository<IndividualReduction> individualReductions,
                                               IEventBus eventBus,
                                               AuthenticatedUserId userId,
                                               IDateTimeProvider dateTimeProvider,
                                               ReadModelUpdater readModelUpdater)
    {
        _registrations = registrations;
        _incomingPayments = incomingPayments;
        _assignments = assignments;
        _individualReductions = individualReductions;
        _eventBus = eventBus;
        _userId = userId;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
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
                             Created = _dateTimeProvider.Now
                         };
        await _assignments.InsertOrUpdateEntity(assignment, cancellationToken);

        if (command.AcceptDifference)
        {
            var difference = registration.Price_AdmittedAndReduced
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
                              IncomingPaymentId = incomingPayment.Id
                          });

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);

        return Unit.Value;
    }
}