using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables;
using MediatR;

namespace EventRegistrar.Backend.Authorization
{
    public class AuthorizationDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IAuthorizationChecker _authorizationChecker;

        public AuthorizationDecorator(IEventAcronymResolver acronymResolver,
                                      IAuthorizationChecker authorizationChecker)
        {
            _acronymResolver = acronymResolver;
            _authorizationChecker = authorizationChecker;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var requestType = request.GetType().Name;
            if (request is IEventBoundRequest eventBoundRequest)
            {
                var eventId = await _acronymResolver.GetEventIdFromAcronym(eventBoundRequest.EventAcronym);
                await _authorizationChecker.ThrowIfUserHasNotRight(eventId, requestType);
            }

            return await next();
        }
    }
}