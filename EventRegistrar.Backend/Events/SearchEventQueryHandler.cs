using System;
using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using MediatR;

namespace EventRegistrar.Backend.Events
{
    public class SearchEventQueryHandler : RequestHandler<SearchEventQuery, IEnumerable<EventSearchResult>>
    {
        private readonly IQueryable<Event> _events;
        private readonly AuthenticatedUser _user;

        public SearchEventQueryHandler(IQueryable<Event> events,
                                       AuthenticatedUser user)
        {
            _events = events;
            _user = user;
        }

        protected override IEnumerable<EventSearchResult> Handle(SearchEventQuery request)
        {
            return _events.Where(evt => evt.Name.Contains(request.SearchString, StringComparison.InvariantCultureIgnoreCase))
                          .WhereIf(!request.IncludeAuthorizedEvents && _user.UserId.HasValue, evt => evt.Users.All(usr => usr.UserId != _user.UserId.Value))
                          .WhereIf(!request.IncludeRequestedEvents && _user.UserId.HasValue, evt => evt.AccessRequests.All(usr => usr.UserId != _user.UserId.Value))
                          .Select(evt => new EventSearchResult
                          {
                              Id = evt.Id,
                              Name = evt.Name,
                              Acronym = evt.Acronym,
                              State = evt.State
                          })
                .ToList();
        }
    }
}