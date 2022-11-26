using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class RespondToRequestCommand : IRequest<Unit>, IEventBoundRequest
{
    public Guid AccessToEventRequestId { get; set; }
    public Guid EventId { get; set; }
    public RequestResponse Response { get; set; }
}

public class RespondToRequestCommandHandler : IRequestHandler<RespondToRequestCommand>
{
    private readonly IRepository<AccessToEventRequest> _accessRequests;
    private readonly IRepository<User> _users;
    private readonly IEventBus _eventBus;
    private readonly IRepository<UserInEvent> _usersInEvents;

    public RespondToRequestCommandHandler(IRepository<AccessToEventRequest> accessRequests,
                                          IRepository<UserInEvent> usersInEvents,
                                          IRepository<User> users,
                                          IEventBus eventBus)
    {
        _accessRequests = accessRequests;
        _usersInEvents = usersInEvents;
        _users = users;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RespondToRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _accessRequests.FirstAsync(req => req.Id == command.AccessToEventRequestId, cancellationToken);
        if (request.Response != null)
        {
            throw new ArgumentException("Request has already been answered");
        }

        request.Response = command.Response;
        if (request.Response == RequestResponse.Granted)
        {
            var userId = await CreateUserIfNecessary(request);
            var userInEvent = new UserInEvent
                              {
                                  Id = Guid.NewGuid(),
                                  UserId = userId,
                                  EventId = request.EventId,
                                  Role = UserInEventRole.Reader
                              };
            await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
        }

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(UsersOfEventQuery)
                          });
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(AccessRequestsOfEventQuery)
                          });
        return Unit.Value;
    }

    private async Task<Guid> CreateUserIfNecessary(AccessToEventRequest request)
    {
        if (request.UserId_Requestor.HasValue)
        {
            return request.UserId_Requestor.Value;
        }

        var user = new User
                   {
                       Id = Guid.NewGuid(),
                       IdentityProvider = request.IdentityProvider,
                       IdentityProviderUserIdentifier = request.Identifier,
                       FirstName = request.FirstName,
                       LastName = request.LastName,
                       Email = request.Email
                   };
        await _users.InsertOrUpdateEntity(user);

        return user.Id;
    }
}