using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events.Context
{
    public class ExtractEventIdDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly EventContext _eventContext;

        public ExtractEventIdDecorator(EventContext eventContext,
                                       IEventAcronymResolver acronymResolver)
        {
            _eventContext = eventContext;
            _acronymResolver = acronymResolver;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is IEventBoundRequest eventBoundRequest &&
                !string.IsNullOrEmpty(eventBoundRequest.EventAcronym))
            {
                _eventContext.EventId = await _acronymResolver.GetEventIdFromAcronym(eventBoundRequest.EventAcronym);
            }

            return await next();
        }
    }
}