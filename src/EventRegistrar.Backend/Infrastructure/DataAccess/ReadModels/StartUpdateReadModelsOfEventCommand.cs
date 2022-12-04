using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Assignments.Candidates;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateReadModelsOfEventCommand : IRequest
{
    public Guid? EventId { get; set; }
    public IEnumerable<string>? QueryNames { get; set; }
}

public class StartUpdateReadModelsOfEventCommandHandler : IRequestHandler<StartUpdateReadModelsOfEventCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Payment> _payments;
    private readonly ReadModelUpdater _readModelUpdater;

    public StartUpdateReadModelsOfEventCommandHandler(IQueryable<Event> events,
                                                      IQueryable<Registration> registrations,
                                                      IQueryable<Payment> payments,
                                                      ReadModelUpdater readModelUpdater)
    {
        _events = events;
        _registrations = registrations;
        _payments = payments;
        _readModelUpdater = readModelUpdater;
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
                _readModelUpdater.TriggerUpdate<RegistrablesOverviewCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(DuePaymentsQuery)) != false)
            {
                _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(RegistrationQuery)) != false)
            {
                var registrationIds = await _registrations.Where(reg => reg.EventId == eventId)
                                                          .Select(reg => reg.Id)
                                                          .ToListAsync(cancellationToken);

                foreach (var registrationId in registrationIds)
                {
                    _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registrationId, eventId);
                }
            }

            if (command.QueryNames?.Contains(nameof(PaymentAssignmentsQuery)) != false)
            {
                var paymentIds = await _payments.Where(pmt => pmt.PaymentsFile!.EventId == eventId)
                                                .Select(pmt => pmt.Id)
                                                .ToListAsync(cancellationToken);

                foreach (var paymentId in paymentIds)
                {
                    _readModelUpdater.TriggerUpdate<PaymentAssignmentsCalculator>(paymentId, eventId);
                }
            }
        }

        return Unit.Value;
    }
}