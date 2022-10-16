using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class EventsOfUserQuery : IRequest<EventsOfUser> { }

public class EventsOfUserQueryHandler : IRequestHandler<EventsOfUserQuery, EventsOfUser>
{
    private readonly IQueryable<AccessToEventRequest> _accessRequests;
    private readonly EnumTranslator _enumTranslator;
    private readonly AuthenticatedUser _authenticatedUser;
    private readonly AuthenticatedUserId _authenticatedUserId;
    private readonly IQueryable<UserInEvent> _usersInEvents;

    public EventsOfUserQueryHandler(IQueryable<UserInEvent> usersInEvents,
                                    AuthenticatedUserId authenticatedUserId,
                                    AuthenticatedUser authenticatedUser,
                                    IQueryable<AccessToEventRequest> accessRequests,
                                    EnumTranslator enumTranslator)
    {
        _usersInEvents = usersInEvents;
        _authenticatedUserId = authenticatedUserId;
        _authenticatedUser = authenticatedUser;
        _accessRequests = accessRequests;
        _enumTranslator = enumTranslator;
    }

    public async Task<EventsOfUser> Handle(EventsOfUserQuery request,
                                           CancellationToken cancellationToken)
    {
        if (_authenticatedUserId.UserId == null && _authenticatedUser == AuthenticatedUser.None)
        {
            return new EventsOfUser
                   {
                       AuthorizedEvents = Enumerable.Empty<EventOfUser>(),
                       Requests = Enumerable.Empty<AccessRequest>()
                   };
        }

        return new EventsOfUser
               {
                   AuthorizedEvents = await _usersInEvents.Where(uie => uie.UserId == _authenticatedUserId.UserId)
                                                          .OrderBy(uie => uie.Event!.State)
                                                          .ThenBy(uie => uie.Event!.Name)
                                                          .Select(uie => new EventOfUser
                                                                         {
                                                                             EventId = uie.EventId,
                                                                             EventName = uie.Event!.Name,
                                                                             EventAcronym = uie.Event.Acronym,
                                                                             EventState = uie.Event.State,
                                                                             EventStateText = _enumTranslator.Translate(uie.Event.State),
                                                                             Role = uie.Role,
                                                                             RoleText = _enumTranslator.Translate(uie.Role)
                                                                         })
                                                          .ToListAsync(cancellationToken),

                   Requests = await _accessRequests.WhereIf(_authenticatedUserId.UserId != null,
                                                            req => req.UserId_Requestor == _authenticatedUserId.UserId)
                                                   .WhereIf(_authenticatedUserId.UserId == null && _authenticatedUser != AuthenticatedUser.None,
                                                            req => req.IdentityProvider == _authenticatedUser.IdentityProvider
                                                                && req.Identifier == _authenticatedUser.IdentityProviderUserIdentifier)
                                                   .Select(req => new AccessRequest
                                                                  {
                                                                      EventId = req.EventId,
                                                                      EventName = req.Event!.Name,
                                                                      EventAcronym = req.Event.Acronym,
                                                                      EventState = req.Event.State,
                                                                      EventStateText = _enumTranslator.Translate(req.Event.State),
                                                                      RequestSent = req.RequestReceived
                                                                  })
                                                   .ToListAsync(cancellationToken)
               };
    }
}

public class EventsOfUser
{
    public IEnumerable<EventOfUser> AuthorizedEvents { get; set; } = null!;
    public IEnumerable<AccessRequest> Requests { get; set; } = null!;
}

public class EventOfUser
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = null!;
    public string EventAcronym { get; set; } = null!;
    public EventState EventState { get; set; }
    public string EventStateText { get; set; } = null!;
    public UserInEventRole Role { get; set; }
    public string RoleText { get; set; } = null!;
}

public class AccessRequest
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = null!;
    public string EventAcronym { get; set; } = null!;
    public EventState EventState { get; set; }
    public string EventStateText { get; set; } = null!;
    public DateTimeOffset RequestSent { get; set; }
}