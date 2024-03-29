﻿namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public interface IEventToCommandTranslation<in TEvent>
    where TEvent : DomainEvent
{
    IEnumerable<IRequest> Translate(TEvent e);
}