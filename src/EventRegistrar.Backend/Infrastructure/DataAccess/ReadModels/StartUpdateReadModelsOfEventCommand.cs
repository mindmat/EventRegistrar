using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateReadModelsOfEventCommand : IRequest
{
    public Guid? EventId { get; set; }
    public IEnumerable<string>? QueryNames { get; set; }
}

public class StartUpdateReadModelsOfEventCommandHandler : AsyncRequestHandler<StartUpdateReadModelsOfEventCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<Registration> _registrations;
    private readonly ChangeTrigger _changeTrigger;

    public StartUpdateReadModelsOfEventCommandHandler(IQueryable<Event> events,
                                                      IQueryable<Registration> registrations,
                                                      ChangeTrigger changeTrigger)
    {
        _events = events;
        _registrations = registrations;
        _changeTrigger = changeTrigger;
    }

    protected override async Task Handle(StartUpdateReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        var eventIds = await _events.WhereIf(command.EventId != null, evt => evt.Id == command.EventId)
                                    .Select(evt => evt.Id)
                                    .ToListAsync(cancellationToken);
        foreach (var eventId in eventIds)
        {
            if (command.QueryNames?.Contains(nameof(RegistrablesOverviewQuery)) != false)
            {
                _changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(DuePaymentsQuery)) != false)
            {
                _changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(RegistrationQuery)) != false)
            {
                var registrationIds = await _registrations.Where(reg => reg.EventId == eventId)
                                                          .Select(reg => reg.Id)
                                                          .ToListAsync(cancellationToken);

                foreach (var registrationId in registrationIds)
                {
                    _changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId, eventId);
                }
            }
        }
    }
}