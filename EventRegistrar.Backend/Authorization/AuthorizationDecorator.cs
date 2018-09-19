using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EventRegistrar.Backend.Authorization
{
    public class AuthorizationDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IAuthorizationChecker _authorizationChecker;

        public AuthorizationDecorator(IAuthorizationChecker authorizationChecker)
        {
            _authorizationChecker = authorizationChecker;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var requestType = request.GetType().Name;
            if (request is IEventBoundRequest eventBoundRequest)
            {
                await _authorizationChecker.ThrowIfUserHasNotRight(eventBoundRequest.EventAcronym, requestType);
            }

            return await next();
        }
    }
}