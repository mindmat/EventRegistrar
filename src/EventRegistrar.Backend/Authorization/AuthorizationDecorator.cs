namespace EventRegistrar.Backend.Authorization;

public class AuthorizationDecorator<TRequest, TResponse>(IAuthorizationChecker authorizationChecker) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name;
        if (request is IEventBoundRequest eventBoundRequest)
        {
            await authorizationChecker.ThrowIfUserHasNotRight(eventBoundRequest.EventId, requestType);
        }

        return await next();
    }
}