namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event, bool publishEvenWhenDbCommitFails = false)
        where TEvent : DomainEvent;
}