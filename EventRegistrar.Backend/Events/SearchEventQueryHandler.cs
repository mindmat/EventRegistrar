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

        public async Task<IEnumerable<EventSearchResult>> Handle(SearchEventQuery request, CancellationToken cancellationToken)
        {
            return await _events.Where(evt => evt.Name.Contains(request.SearchString, StringComparison.InvariantCultureIgnoreCase))
                                .WhereIf(!request.IncludeAuthorizedEvents && _userId.UserId.HasValue, evt => evt.Users.All(usr => usr.UserId != _userId.UserId))
                                .WhereIf(!request.IncludeRequestedEvents && _userId.UserId.HasValue, evt => evt.AccessRequests.All(usr => usr.UserId_Requestor != _userId.UserId))
                                .WhereIf(!request.IncludeRequestedEvents && !_userId.UserId.HasValue, evt => evt.AccessRequests.All(usr => usr.IdentityProvider == _user.IdentityProvider
                                                                                                                                        && usr.Identifier == _user.IdentityProviderUserIdentifier))
                                .Select(evt => new EventSearchResult
                                {
                                    Id = evt.Id,
                                    Name = evt.Name,
                                    Acronym = evt.Acronym,
                                    State = evt.State,
                                    RequestSent = _userId.UserId.HasValue && evt.AccessRequests.Any(usr => usr.UserId_Requestor == _userId.UserId.Value) ||
                                                  !_userId.UserId.HasValue && evt.AccessRequests.Any(usr => usr.IdentityProvider == _user.IdentityProvider
                                                                                                         && usr.Identifier == _user.IdentityProviderUserIdentifier)
                                })
                                .ToListAsync(cancellationToken);
        }
    }
}