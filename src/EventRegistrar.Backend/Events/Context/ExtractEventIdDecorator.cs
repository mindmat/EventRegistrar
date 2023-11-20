namespace EventRegistrar.Backend.Events.Context;

public class ExtractEventIdDecorator<TRequest, TResponse>(EventContext eventContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        if (request is IEventBoundRequest eventBoundRequest)
        {
            eventContext.EventId = eventBoundRequest.EventId;
        }

        return await next();
    }
}