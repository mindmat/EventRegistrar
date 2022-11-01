using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateReadModelsOfEventCommand : IRequest
{
    public Guid? EventId { get; set; }
    public IEnumerable<string>? QueryNames { get; set; }
}

public class StartUpdateReadModelsOfEventCommandHandler : IRequestHandler<StartUpdateReadModelsOfEventCommand>
{
    private readonly CommandQueue _commandQueue;
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Payment> _payments;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartUpdateReadModelsOfEventCommandHandler(CommandQueue commandQueue,
                                                      IQueryable<Event> events,
                                                      IQueryable<Registration> registrations,
                                                      IQueryable<Payment> payments,
                                                      IDateTimeProvider dateTimeProvider)
    {
        _commandQueue = commandQueue;
        _events = events;
        _registrations = registrations;
        _payments = payments;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(StartUpdateReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        var eventIds = await _events.WhereIf(command.EventId != null, evt => evt.Id == command.EventId)
                                    .Select(evt => evt.Id)
                                    .ToListAsync(cancellationToken);
        foreach (var eventId in eventIds)
        {
            if (command.QueryNames?.Contains(nameof(RegistrablesOverviewQuery)) != false)
            {
                _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                             {
                                                 QueryName = nameof(RegistrablesOverviewQuery),
                                                 EventId = eventId,
                                                 DirtyMoment = _dateTimeProvider.Now
                                             });
            }

            if (command.QueryNames?.Contains(nameof(DuePaymentsQuery)) != false)
            {
                _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                             {
                                                 QueryName = nameof(DuePaymentsQuery),
                                                 EventId = eventId,
                                                 DirtyMoment = _dateTimeProvider.Now
                                             });
            }

            if (command.QueryNames?.Contains(nameof(RegistrationQuery)) != false)
            {
                var registrationIds = await _registrations.Where(reg => reg.EventId == eventId)
                                                          .Select(reg => reg.Id)
                                                          .ToListAsync(cancellationToken);

                foreach (var registrationId in registrationIds)
                {
                    _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                                 {
                                                     QueryName = nameof(RegistrationQuery),
                                                     EventId = eventId,
                                                     RowId = registrationId,
                                                     DirtyMoment = _dateTimeProvider.Now
                                                 });
                }
            }

            if (command.QueryNames?.Contains(nameof(PaymentAssignmentsQuery)) != false)
            {
                var paymentIds = await _payments.Where(pmt => pmt.PaymentsFile!.EventId == eventId)
                                                .Select(pmt => pmt.Id)
                                                .ToListAsync(cancellationToken);

                foreach (var paymentId in paymentIds)
                {
                    _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                                 {
                                                     QueryName = nameof(PaymentAssignmentsQuery),
                                                     EventId = eventId,
                                                     RowId = paymentId,
                                                     DirtyMoment = _dateTimeProvider.Now
                                                 });
                }
            }
        }

        return Unit.Value;
    }
}