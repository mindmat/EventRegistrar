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

public class StartUpdateReadModelsOfEventCommandHandler(IQueryable<Event> events,
                                                        IQueryable<Registration> registrations,
                                                        ChangeTrigger changeTrigger)
    : IRequestHandler<StartUpdateReadModelsOfEventCommand>
{
    public async Task Handle(StartUpdateReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        var eventIds = await events.WhereIf(command.EventId != null, evt => evt.Id == command.EventId)
                                   .Select(evt => evt.Id)
                                   .ToListAsync(cancellationToken);
        foreach (var eventId in eventIds)
        {
            if (command.QueryNames?.Contains(nameof(RegistrablesOverviewQuery)) != false)
            {
                changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(DuePaymentsQuery)) != false)
            {
                changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, eventId);
            }

            if (command.QueryNames?.Contains(nameof(RegistrationQuery)) != false)
            {
                var registrationIds = await registrations.Where(reg => reg.EventId == eventId)
                                                         .Select(reg => reg.Id)
                                                         .ToListAsync(cancellationToken);

                foreach (var registrationId in registrationIds)
                {
                    changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId, eventId);
                }
            }
        }
    }
}