using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class EventBus(Container container,
                      CommandQueue commandQueue,
                      EventContext eventContext,
                      AuthenticatedUserId user,
                      IHubContext<NotificationHub, INotificationConsumer> hub)
    : IEventBus
{
    private readonly IList<QueryChanged> _notifications = new List<QueryChanged>();

    public void Publish<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        // try to fill out missing data
        if (@event.Id == Guid.Empty)
        {
            @event.Id = Guid.NewGuid();
        }

        @event.UserId ??= user.UserId;
        @event.EventId ??= eventContext.EventId;

        var translations = container.GetAllInstances<IEventToCommandTranslation<TEvent>>().ToList();
        foreach (var command in translations.SelectMany(trn => trn.Translate(@event)))
        {
            commandQueue.EnqueueCommand(command);
        }

        if (@event is QueryChanged queryChangedEvent)
        {
            _notifications.Add(queryChangedEvent);
        }
        else
        {
            commandQueue.EnqueueCommand(new SaveDomainEventCommand
                                        {
                                            DomainEventId = @event.Id,
                                            DomainEventId_Parent = @event.DomainEventId_Parent,
                                            EventId = @event.EventId ?? eventContext.EventId,
                                            EventType = @event.GetType().FullName!,
                                            EventData = JsonConvert.SerializeObject(@event)
                                        });
        }
    }

    public void Release()
    {
        foreach (var notification in _notifications)
        {
            if (notification.EventId != null)
            {
                hub.Clients.Group(notification.EventId!.ToString()!)
                   .Process(notification.EventId!.Value, notification.QueryName, notification.RowId);
            }
            else
            {
                hub.Clients.All.Process(null, notification.QueryName, notification.RowId);
            }
        }
    }
}