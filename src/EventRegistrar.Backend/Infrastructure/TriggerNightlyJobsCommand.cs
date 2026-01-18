using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Mailing.Templates.Validation;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Statistics;

using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Infrastructure;

public class TriggerNightlyJobsCommand : IRequest;

public class TriggerNightlyJobsCommandHandler(IQueryable<Event> events,
                                              ChangeTrigger changeTrigger,
                                              ILogger<TriggerNightlyJobsCommandHandler> logger)
    : IRequestHandler<TriggerNightlyJobsCommand>
{
    public async Task Handle(TriggerNightlyJobsCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting nightly jobs trigger");

        // Get active events for regular nightly jobs
        var activeEventIds = await events.Where(evt => evt.State != EventState.Finished)
                                         .Select(evt => evt.Id)
                                         .ToListAsync(cancellationToken);

        logger.LogInformation("Found {EventCount} active events for nightly jobs", activeEventIds.Count);

        // Trigger regular nightly jobs per event
        foreach (var eventId in activeEventIds)
        {
            changeTrigger.EnqueueCommand(new CheckExternalMailConfigurationCommand { EventId = eventId });
            changeTrigger.EnqueueCommand(new ValidateAutoMailTemplatesCommand { EventId = eventId });
        }

        // Trigger statistics collection for all calculators
        changeTrigger.EnqueueCommand(new CollectStatisticsCommand());

        logger.LogInformation("Nightly jobs trigger completed. Enqueued {EventJobs} event-specific jobs and triggered statistics collection",
            activeEventIds.Count * 2);
    }
}