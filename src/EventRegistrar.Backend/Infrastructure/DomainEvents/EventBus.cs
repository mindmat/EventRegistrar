using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class EventBus : IEventBus
{
    private readonly Container _container;
    private readonly EventContext _eventContext;
    private readonly CommandQueue _commandQueue;
    private readonly AuthenticatedUserId _user;
    private readonly IHubContext<NotificationHub, INotificationConsumer> _hub;
    private readonly IList<ReadModelUpdated> _notifications = new List<ReadModelUpdated>();

    public EventBus(Container container,
                    CommandQueue commandQueue,
                    EventContext eventContext,
                    AuthenticatedUserId user,
                    IHubContext<NotificationHub, INotificationConsumer> hub)
    {
        _container = container;
        _commandQueue = commandQueue;
        _eventContext = eventContext;
        _user = user;
        _hub = hub;
    }

    public void Publish<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        // try to fill out missing data
        if (@event.Id == Guid.Empty)
        {
            @event.Id = Guid.NewGuid();
        }

        @event.UserId ??= _user.UserId;
        @event.EventId ??= _eventContext.EventId;

        var translations = _container.GetAllInstances<IEventToCommandTranslation<TEvent>>().ToList();
        foreach (var command in translations.SelectMany(trn => trn.Translate(@event)))
        {
            _commandQueue.EnqueueCommand(command);
        }

        if (@event is ReadModelUpdated readModelUpdatedEvent)
        {
            _notifications.Add(readModelUpdatedEvent);
        }
        else
        {
            _commandQueue.EnqueueCommand(new SaveDomainEventCommand
                                         {
                                             DomainEventId = @event.Id,
                                             DomainEventId_Parent = @event.DomainEventId_Parent,
                                             EventId = @event.EventId ?? _eventContext.EventId,
                                             EventType = @event.GetType().FullName!,
                                             EventData = JsonConvert.SerializeObject(@event)
                                         });
        }
    }

    public void Release()
    {
        foreach (var notification in _notifications)
        {
            _hub.Clients.Group(notification.EventId!.ToString()!)
                .Process(notification.EventId!.Value, notification.QueryName, notification.RowId);
        }
    }
}