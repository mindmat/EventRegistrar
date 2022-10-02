using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class RequestAccessCommand : IRequest<Guid>
{
    public Guid EventId { get; set; }
    public string? RequestText { get; set; }
}

public class RequestAccessCommandHandler : IRequestHandler<RequestAccessCommand, Guid>
{
    private readonly IRepository<AccessToEventRequest> _accessRequests;
    private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
    private readonly IEventBus _eventBus;
    private readonly AuthenticatedUserId _user;

    public RequestAccessCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                       AuthenticatedUserId user,
                                       IAuthenticatedUserProvider authenticatedUserProvider,
                                       IEventBus eventBus)
    {
        _accessRequests = accessRequests;
        _user = user;
        _authenticatedUserProvider = authenticatedUserProvider;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
    {
        var requestExpression = _accessRequests.Where(req => req.EventId == command.EventId
                                                          && (req.Response == null || req.Response == RequestResponse.Granted));
        var existingUserId = await _authenticatedUserProvider.GetAuthenticatedUserId();
        if (existingUserId != null)
        {
            requestExpression = requestExpression.Where(req => req.UserId_Requestor == existingUserId);
        }
        else
        {
            var authenticatedUser = _authenticatedUserProvider.GetAuthenticatedUser();
            if (authenticatedUser.IdentityProviderUserIdentifier == null)
            {
                throw new ArgumentException("You are not authenticated");
            }

            requestExpression = requestExpression.Where(req => req.IdentityProvider == authenticatedUser.IdentityProvider
                                                            && req.Identifier == authenticatedUser.IdentityProviderUserIdentifier);
        }

        var request = await requestExpression.FirstOrDefaultAsync(cancellationToken);

        if (request == null)
        {
            var user = _authenticatedUserProvider.GetAuthenticatedUser();
            request = new AccessToEventRequest
                      {
                          Id = Guid.NewGuid(),
                          UserId_Requestor = _user.UserId,
                          IdentityProvider = user.IdentityProvider,
                          Identifier = user.IdentityProviderUserIdentifier,
                          FirstName = user.FirstName,
                          LastName = user.LastName,
                          Email = user.Email,
                          RequestText = command.RequestText,
                          EventId = command.EventId,
                          RequestReceived = DateTime.UtcNow
                      };
            _accessRequests.InsertObjectTree(request);

            _eventBus.Publish(new ReadModelUpdated
                              {
                                  QueryName = nameof(EventsOfUserQuery)
                              });
            _eventBus.Publish(new ReadModelUpdated
                              {
                                  EventId = command.EventId,
                                  QueryName = nameof(AccessRequestsOfEventQuery)
                              });
        }

        return request.Id;
    }
}