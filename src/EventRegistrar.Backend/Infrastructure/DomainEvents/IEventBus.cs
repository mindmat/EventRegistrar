namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event)
        where TEvent : DomainEvent;
}