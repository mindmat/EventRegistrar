using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Events;

public class SearchEventQuery : IRequest<IEnumerable<EventSearchResult>>
{
    public bool IncludeAuthorizedEvents { get; set; }
    public bool IncludeRequestedEvents { get; set; }
    public string? SearchString { get; set; }
}

public class SearchEventQueryHandler(IQueryable<Event> events,
                                     AuthenticatedUserId userId,
                                     AuthenticatedUser user,
                                     EnumTranslator enumTranslator)
    : IRequestHandler<SearchEventQuery, IEnumerable<EventSearchResult>>
{
    public async Task<IEnumerable<EventSearchResult>> Handle(SearchEventQuery query,
                                                             CancellationToken cancellationToken)
    {
        return await events.WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                    evt => EF.Functions.Like(evt.Name, $"%{query.SearchString}%")
                                        || EF.Functions.Like(evt.Acronym, $"%{query.SearchString}%"))
                           .WhereIf(!query.IncludeAuthorizedEvents && userId.UserId != null,
                                    evt => evt.Users!.All(usr => usr.UserId != userId.UserId))
                           .WhereIf(!query.IncludeRequestedEvents && userId.UserId != null,
                                    evt => evt.AccessRequests!.All(usr => usr.UserId_Requestor != userId.UserId))
                           .WhereIf(!query.IncludeRequestedEvents && userId.UserId == null,
                                    evt => !evt.AccessRequests!.Any(usr => usr.IdentityProvider == user.IdentityProvider
                                                                        && usr.Identifier == user.IdentityProviderUserIdentifier))
                           .OrderBy(evt => evt.State)
                           .ThenBy(evt => evt.Name)
                           .Take(20)
                           .Select(evt => new EventSearchResult
                                          {
                                              Id = evt.Id,
                                              Name = evt.Name,
                                              Acronym = evt.Acronym,
                                              State = evt.State,
                                              StateText = enumTranslator.Translate(evt.State),
                                              RequestSent = (userId.UserId != null
                                                          && evt.AccessRequests!.Any(req => req.UserId_Requestor == userId.UserId
                                                                                         && req.Response == null))
                                                         || (userId.UserId == null
                                                          && evt.AccessRequests!.Any(req => req.IdentityProvider == user.IdentityProvider
                                                                                         && req.Identifier == user.IdentityProviderUserIdentifier
                                                                                         && req.Response == null))
                                          })
                           .ToListAsync(cancellationToken);
    }
}