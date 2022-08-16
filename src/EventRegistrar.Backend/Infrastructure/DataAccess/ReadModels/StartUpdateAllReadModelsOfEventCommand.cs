using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateAllReadModelsOfEventCommand : IRequest
{
    public Guid EventId { get; set; }
}

public class StartUpdateAllReadModelsOfEventCommandHandler : IRequestHandler<StartUpdateAllReadModelsOfEventCommand>
{
    private readonly CommandQueue _commandQueue;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Payment> _payments;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartUpdateAllReadModelsOfEventCommandHandler(CommandQueue commandQueue,
                                                         IQueryable<Registration> registrations,
                                                         IQueryable<Payment> payments,
                                                         IDateTimeProvider dateTimeProvider)
    {
        _commandQueue = commandQueue;
        _registrations = registrations;
        _payments = payments;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(StartUpdateAllReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = nameof(RegistrablesOverviewQuery),
                                         EventId = command.EventId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });

        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = nameof(DuePaymentsQuery),
                                         EventId = command.EventId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });

        var registrationIds = await _registrations.Where(reg => reg.EventId == command.EventId)
                                                  .Select(reg => reg.Id)
                                                  .ToListAsync(cancellationToken);

        foreach (var registrationId in registrationIds)
        {
            _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                         {
                                             QueryName = nameof(RegistrationQuery),
                                             EventId = command.EventId,
                                             RowId = registrationId,
                                             DirtyMoment = _dateTimeProvider.Now
                                         });
        }

        var paymentIds = await _payments.Where(pmt => pmt.PaymentsFile!.EventId == command.EventId)
                                        .Select(pmt => pmt.Id)
                                        .ToListAsync(cancellationToken);

        foreach (var paymentId in paymentIds)
        {
            _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                         {
                                             QueryName = nameof(PaymentAssignmentsQuery),
                                             EventId = command.EventId,
                                             RowId = paymentId,
                                             DirtyMoment = _dateTimeProvider.Now
                                         });
        }


        return Unit.Value;
    }
}