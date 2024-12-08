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
    private readonly List<(QueryChanged Event, bool PublishAnyway)> _notifications = [];

    public void Publish<TEvent>(TEvent @event, bool publishEvenWhenDbCommitFails = false)
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
            _notifications.Add((queryChangedEvent, publishEvenWhenDbCommitFails));
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

    public void Release(bool dbCommitSucceeded)
    {
        foreach (var notification in _notifications.Where(ntf => dbCommitSucceeded || ntf.PublishAnyway))
        {
            if (notification.Event.EventId != null)
            {
                hub.Clients.Group(notification.Event.EventId!.ToString()!)
                   .Process(notification.Event.EventId!.Value, notification.Event.QueryName, notification.Event.RowId);
            }
            else
            {
                hub.Clients.All.Process(null, notification.Event.QueryName, notification.Event.RowId);
            }
        }
    }
}