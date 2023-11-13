using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ChangeTrigger
{
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly EventContext _eventContext;
    private readonly IEnumerable<IReadModelCalculator> _calculators;
    private readonly IEventBus _eventBus;

    public ChangeTrigger(CommandQueue commandQueue,
                         IDateTimeProvider dateTimeProvider,
                         EventContext eventContext,
                         IEnumerable<IReadModelCalculator> calculators,
                         IEventBus eventBus)
    {
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _eventContext = eventContext;
        _calculators = calculators;
        _eventBus = eventBus;
    }

    public void TriggerUpdate<T>(Guid? rowId = null, Guid? eventId = null)
        where T : IReadModelCalculator
    {
        eventId ??= _eventContext.EventId;
        if (eventId == null)
        {
            throw new ArgumentNullException(nameof(eventId));
        }

        var queryName = _calculators.First(cal => cal.GetType() == typeof(T))
                                    .QueryName;

        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = queryName,
                                         EventId = eventId.Value,
                                         RowId = rowId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });
    }

    public void QueryChanged<TQuery>(Guid eventId, Guid? rowId = null)
        where TQuery : IEventBoundRequest
    {
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = eventId,
                              QueryName = typeof(TQuery).Name,
                              RowId = rowId
                          });
    }

    public void PublishEvent<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        _eventBus.Publish(@event);
    }

    public void EnqueueCommand<T>(T command)
        where T : IRequest
    {
        _commandQueue.EnqueueCommand(command);
    }
}