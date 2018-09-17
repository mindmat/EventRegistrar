using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public interface IEventToCommandTranslation<TEvent>
      where TEvent : Event
    {
        //Type EventType { get; }

        IQueueBoundMessage Translate(TEvent @event);
    }

    //public abstract class EventToCommandTranslation<TEvent, TCommand> : IEventToCommandTranslation<TEvent>
    //  where TEvent : Event
    //  where TCommand : IRequest, IQueueBoundMessage
    //{
    //    public Type EventType => typeof(TEvent);

    //    IQueueBoundMessage IEventToCommandTranslation.Translate(TEvent @event)
    //    {
    //        return Translate((TEvent)@event);
    //    }

    //    protected abstract TCommand Translate(TEvent @event);

    //    IQueueBoundMessage IEventToCommandTranslation<TEvent>.Translate(TEvent @event)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}