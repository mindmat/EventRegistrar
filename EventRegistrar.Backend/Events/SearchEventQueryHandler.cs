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
        private readonly AuthenticatedUserId _user;

        public SearchEventQueryHandler(IQueryable<Event> events,
                                       AuthenticatedUserId user)
        {
            _events = events;
            _user = user;
        }

        public async Task<IEnumerable<EventSearchResult>> Handle(SearchEventQuery request, CancellationToken cancellationToken)
        {
            return await _events.Where(evt => evt.Name.Contains(request.SearchString, StringComparison.InvariantCultureIgnoreCase))
                                .WhereIf(!request.IncludeAuthorizedEvents && _user.UserId.HasValue, evt => evt.Users.All(usr => usr.UserId != _user.UserId))
                                .WhereIf(!request.IncludeRequestedEvents && _user.UserId.HasValue, evt => evt.AccessRequests.All(usr => usr.UserId_Requestor != _user.UserId))
                                .Select(evt => new EventSearchResult
                                {
                                    Id = evt.Id,
                                    Name = evt.Name,
                                    Acronym = evt.Acronym,
                                    State = evt.State,
                                    RequestSent = evt.AccessRequests.Any(usr => usr.UserId_Requestor == _user.UserId.Value)
                                })
                                .ToListAsync(cancellationToken);
        }
    }
}