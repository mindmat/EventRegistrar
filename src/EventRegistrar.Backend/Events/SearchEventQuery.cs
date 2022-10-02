using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Events;

public class SearchEventQuery : IRequest<IEnumerable<EventSearchResult>>
{
    public bool IncludeAuthorizedEvents { get; set; }
    public bool IncludeRequestedEvents { get; set; }
    public string? SearchString { get; set; }
}

public class SearchEventQueryHandler : IRequestHandler<SearchEventQuery, IEnumerable<EventSearchResult>>
{
    private readonly IQueryable<Event> _events;
    private readonly AuthenticatedUser _user;
    private readonly EnumTranslator _enumTranslator;
    private readonly AuthenticatedUserId _userId;

    public SearchEventQueryHandler(IQueryable<Event> events,
                                   AuthenticatedUserId userId,
                                   AuthenticatedUser user,
                                   EnumTranslator enumTranslator)
    {
        _events = events;
        _userId = userId;
        _user = user;
        _enumTranslator = enumTranslator;
    }

    public async Task<IEnumerable<EventSearchResult>> Handle(SearchEventQuery query,
                                                             CancellationToken cancellationToken)
    {
        return await _events.WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                     evt => EF.Functions.Like(evt.Name, $"%{query.SearchString}%")
                                         || EF.Functions.Like(evt.Acronym, $"%{query.SearchString}%"))
                            .WhereIf(!query.IncludeAuthorizedEvents && _userId.UserId != null,
                                     evt => evt.Users!.All(usr => usr.UserId != _userId.UserId))
                            .WhereIf(!query.IncludeRequestedEvents && _userId.UserId != null,
                                     evt => evt.AccessRequests!.All(usr => usr.UserId_Requestor != _userId.UserId))
                            .WhereIf(!query.IncludeRequestedEvents && _userId.UserId == null,
                                     evt => !evt.AccessRequests!.Any(usr => usr.IdentityProvider == _user.IdentityProvider
                                                                         && usr.Identifier == _user.IdentityProviderUserIdentifier))
                            .OrderBy(evt => evt.State)
                            .ThenBy(evt => evt.Name)
                            .Take(20)
                            .Select(evt => new EventSearchResult
                                           {
                                               Id = evt.Id,
                                               Name = evt.Name,
                                               Acronym = evt.Acronym,
                                               State = evt.State,
                                               StateText = _enumTranslator.Translate(evt.State),
                                               RequestSent = (_userId.UserId != null
                                                           && evt.AccessRequests!.Any(req => req.UserId_Requestor == _userId.UserId
                                                                                          && req.Response == null))
                                                          || (_userId.UserId == null
                                                           && evt.AccessRequests!.Any(req => req.IdentityProvider == _user.IdentityProvider
                                                                                          && req.Identifier == _user.IdentityProviderUserIdentifier
                                                                                          && req.Response == null))
                                           })
                            .ToListAsync(cancellationToken);
    }
}