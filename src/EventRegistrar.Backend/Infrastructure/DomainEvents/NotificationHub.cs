using Microsoft.AspNetCore.SignalR;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public interface INotificationConsumer
{
    Task Process(Guid eventId, string queryName, Guid? rowId);
}

public class NotificationHub : Hub<INotificationConsumer>
{
    public Task SubscribeToEvent(Guid? eventId)
    {
        if (eventId != null)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, eventId.Value.ToString());
        }

        return Task.CompletedTask;
    }

    public Task UnsubscribeFromEvent(Guid? eventId)
    {
        if (eventId != null)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId.Value.ToString());
        }

        return Task.CompletedTask;
    }
}