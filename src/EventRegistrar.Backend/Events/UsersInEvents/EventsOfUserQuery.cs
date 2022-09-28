using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class EventsOfUserQuery : IRequest<IEnumerable<EventOfUser>>
{
    public bool IncludeRequestedEvents { get; set; }
}

public class EventOfUser
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = null!;
    public string EventAcronym { get; set; } = null!;
    public State EventState { get; set; }
    public UserInEventRole Role { get; set; }
    public DateTime RequestSent { get; set; }
}

public class EventsOfUserQueryHandler : IRequestHandler<EventsOfUserQuery, IEnumerable<EventOfUser>>
{
    private readonly IQueryable<AccessToEventRequest> _accessRequests;
    private readonly AuthenticatedUser _authenticatedUser;
    private readonly AuthenticatedUserId _authenticatedUserId;
    private readonly IQueryable<UserInEvent> _usersInEvents;

    public EventsOfUserQueryHandler(IQueryable<UserInEvent> usersInEvents,
                                    AuthenticatedUserId authenticatedUserId,
                                    AuthenticatedUser authenticatedUser,
                                    IQueryable<AccessToEventRequest> accessRequests)
    {
        _usersInEvents = usersInEvents;
        _authenticatedUserId = authenticatedUserId;
        _authenticatedUser = authenticatedUser;
        _accessRequests = accessRequests;
    }

    public async Task<IEnumerable<EventOfUser>> Handle(EventsOfUserQuery request,
                                                       CancellationToken cancellationToken)
    {
        var authorizedEvents = await _usersInEvents.Where(uie => uie.UserId == _authenticatedUserId.UserId)
                                                   .OrderBy(uie => uie.Event!.State)
                                                   .ThenBy(uie => uie.Event!.Name)
                                                   .Select(uie => new EventOfUser
                                                                  {
                                                                      EventId = uie.EventId,
                                                                      EventName = uie.Event!.Name,
                                                                      EventAcronym = uie.Event.Acronym,
                                                                      EventState = uie.Event.State,
                                                                      Role = uie.Role
                                                                  })
                                                   .ToListAsync(cancellationToken);
        if (request.IncludeRequestedEvents && (_authenticatedUserId.UserId.HasValue || _authenticatedUser != AuthenticatedUser.None))
        {
            var requestedEvents = await _accessRequests.WhereIf(_authenticatedUserId.UserId != null,
                                                                req => req.UserId_Requestor == _authenticatedUserId.UserId!.Value)
                                                       .WhereIf(!_authenticatedUserId.UserId.HasValue && _authenticatedUser != AuthenticatedUser.None,
                                                                req => req.IdentityProvider == _authenticatedUser.IdentityProvider
                                                                    && req.Identifier == _authenticatedUser.IdentityProviderUserIdentifier)
                                                       .Select(req => new EventOfUser
                                                                      {
                                                                          EventId = req.EventId,
                                                                          EventName = req.Event!.Name,
                                                                          EventAcronym = req.Event.Acronym,
                                                                          EventState = req.Event.State,
                                                                          RequestSent = req.RequestReceived
                                                                      })
                                                       .ToListAsync(cancellationToken);
            authorizedEvents.AddRange(requestedEvents);
        }

        return authorizedEvents;
    }
}