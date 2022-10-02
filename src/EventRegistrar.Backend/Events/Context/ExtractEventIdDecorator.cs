﻿namespace EventRegistrar.Backend.Events.Context;

public class ExtractEventIdDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly EventContext _eventContext;

    public ExtractEventIdDecorator(EventContext eventContext)
    {
        _eventContext = eventContext;
    }

    public async Task<TResponse> Handle(TRequest request,
                                        CancellationToken cancellationToken,
                                        RequestHandlerDelegate<TResponse> next)
    {
        if (request is IEventBoundRequest eventBoundRequest)
        {
            _eventContext.EventId = eventBoundRequest.EventId;
        }

        return await next();
    }
}