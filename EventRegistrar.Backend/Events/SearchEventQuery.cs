using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events
{
    public class SearchEventQuery : IRequest<IEnumerable<EventSearchResult>>
    {
        public bool IncludeAuthorizedEvents { get; set; }
        public bool IncludeRequestedEvents { get; set; }
        public string SearchString { get; set; }
    }

    public class SearchEventQueryHandler : IRequestHandler<SearchEventQuery, IEnumerable<EventSearchResult>>
    {
        private readonly IQueryable<Event> _events;
        private readonly AuthenticatedUser _user;
        private readonly AuthenticatedUserId _userId;

        public SearchEventQueryHandler(IQueryable<Event> events,
                                       AuthenticatedUserId userId,
                                       AuthenticatedUser user)
        {
            _events = events;
            _userId = userId;
            _user = user;
        }

        public async Task<IEnumerable<EventSearchResult>> Handle(SearchEventQuery query, CancellationToken cancellationToken)
        {
            return await _events.WhereIf(!string.IsNullOrEmpty(query.SearchString), evt => evt.Name.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
                                .WhereIf(!query.IncludeAuthorizedEvents && _userId.UserId.HasValue, evt => evt.Users.All(usr => usr.UserId != _userId.UserId))
                                .WhereIf(!query.IncludeRequestedEvents && _userId.UserId.HasValue, evt => evt.AccessRequests.All(usr => usr.UserId_Requestor != _userId.UserId))
                                .WhereIf(!query.IncludeRequestedEvents && !_userId.UserId.HasValue, evt => !evt.AccessRequests.Any(usr => usr.IdentityProvider == _user.IdentityProvider
                                                                                                                                       && usr.Identifier == _user.IdentityProviderUserIdentifier))
                                .OrderBy(evt => evt.State)
                                .Take(20)
                                .Select(evt => new EventSearchResult
                                {
                                    Id = evt.Id,
                                    Name = evt.Name,
                                    Acronym = evt.Acronym,
                                    State = evt.State,
                                    RequestSent = _userId.UserId.HasValue && evt.AccessRequests.Any(req => req.UserId_Requestor == _userId.UserId.Value
                                                                                                        && !req.Response.HasValue) ||
                                                  !_userId.UserId.HasValue && evt.AccessRequests.Any(req => req.IdentityProvider == _user.IdentityProvider
                                                                                                         && req.Identifier == _user.IdentityProviderUserIdentifier
                                                                                                         && !req.Response.HasValue)
                                })
                                .ToListAsync(cancellationToken);
        }
    }
}