using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class RequestAccessCommand : IRequest<Guid>
{
    public Guid EventId { get; set; }
    public string? RequestText { get; set; }
}

public class RequestAccessCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                         AuthenticatedUserId _user,
                                         IAuthenticatedUserProvider authenticatedUserProvider,
                                         IEventBus eventBus,
                                         CommandQueue commandQueue,
                                         IDateTimeProvider dateTimeProvider)
    : IRequestHandler<RequestAccessCommand, Guid>
{
    public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
    {
        var requestExpression = accessRequests.Where(req => req.EventId == command.EventId
                                                         && (req.Response == null || req.Response == RequestResponse.Granted));
        var existingUserId = await authenticatedUserProvider.GetAuthenticatedUserId();
        if (existingUserId != null)
        {
            requestExpression = requestExpression.Where(req => req.UserId_Requestor == existingUserId);
        }
        else
        {
            var authenticatedUser = authenticatedUserProvider.GetAuthenticatedUser();
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
            var user = authenticatedUserProvider.GetAuthenticatedUser();
            request = new AccessToEventRequest
                      {
                          Id = Guid.NewGuid(),
                          UserId_Requestor = _user.UserId,
                          IdentityProvider = user.IdentityProvider,
                          Identifier = user.IdentityProviderUserIdentifier,
                          FirstName = user.FirstName,
                          LastName = user.LastName,
                          Email = user.Email,
                          AvatarUrl = user.AvatarUrl,
                          RequestText = command.RequestText,
                          EventId = command.EventId,
                          RequestReceived = dateTimeProvider.Now
                      };
            accessRequests.InsertObjectTree(request);

            eventBus.Publish(new QueryChanged
                             {
                                 QueryName = nameof(EventsOfUserQuery)
                             });
            eventBus.Publish(new QueryChanged
                             {
                                 EventId = command.EventId,
                                 QueryName = nameof(AccessRequestsOfEventQuery)
                             });

            if (string.IsNullOrEmpty(user.FirstName)
             || string.IsNullOrEmpty(user.LastName)
             || string.IsNullOrEmpty(user.Email))
            {
                commandQueue.EnqueueCommand(new UpdateUserInfoCommand
                                            {
                                                Provider = request.IdentityProvider,
                                                Identifier = request.Identifier
                                            });
            }
        }

        return request.Id;
    }
}