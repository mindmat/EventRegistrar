using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ChangeTrigger(CommandQueue commandQueue,
                           IDateTimeProvider dateTimeProvider,
                           EventContext eventContext,
                           IEnumerable<IReadModelCalculator> calculators,
                           IEventBus eventBus)
{
    public void TriggerUpdate<T>(Guid? rowId = null, Guid? eventId = null)
        where T : IReadModelCalculator
    {
        eventId ??= eventContext.EventId;
        if (eventId == null)
        {
            throw new ArgumentNullException(nameof(eventId));
        }

        var queryName = calculators.First(cal => cal.GetType() == typeof(T))
                                   .QueryName;

        commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                    {
                                        QueryName = queryName,
                                        EventId = eventId.Value,
                                        RowId = rowId,
                                        DirtyMoment = dateTimeProvider.Now
                                    });
    }

    public void QueryChanged<TQuery>(Guid eventId, Guid? rowId = null)
        where TQuery : IEventBoundRequest
    {
        eventBus.Publish(new QueryChanged
                         {
                             EventId = eventId,
                             QueryName = typeof(TQuery).Name,
                             RowId = rowId
                         });
    }

    public void PublishEvent<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        eventBus.Publish(@event);
    }

    public void EnqueueCommand<T>(T command)
        where T : IRequest
    {
        commandQueue.EnqueueCommand(command);
    }
}