using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Infrastructure;

public class TriggerNightlyJobsCommand : IRequest;

public class TriggerNightlyJobsCommandHandler(IQueryable<Event> events,
                                              ChangeTrigger changeTrigger)
    : IRequestHandler<TriggerNightlyJobsCommand>
{
    public Task Handle(TriggerNightlyJobsCommand command, CancellationToken cancellationToken)
    {
        var activeEventIds = events.Where(evt => evt.State != EventState.Finished)
                                   .Select(evt => evt.Id)
                                   .ToList();
        foreach (var eventId in activeEventIds)
        {
            changeTrigger.EnqueueCommand(new CheckExternalMailConfigurationCommand { EventId = eventId });
        }

        return Task.CompletedTask;
    }
}