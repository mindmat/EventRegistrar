using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class EventsOfUserQueryHandler : IRequestHandler<EventsOfUserQuery, IEnumerable<UserInEventDisplayItem>>
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

        public async Task<IEnumerable<UserInEventDisplayItem>> Handle(EventsOfUserQuery request, CancellationToken cancellationToken)
        {
            var authorizedEvents = await _usersInEvents.Where(uie => uie.UserId == _authenticatedUserId.UserId)
                                                       .Select(uie => new UserInEventDisplayItem
                                                       {
                                                           EventId = uie.EventId,
                                                           EventName = uie.Event.Name,
                                                           EventAcronym = uie.Event.Acronym,
                                                           EventState = uie.Event.State,
                                                           Role = uie.Role,
                                                           UserFirstName = uie.User.FirstName,
                                                           UserLastName = uie.User.LastName,
                                                           UserEmail = uie.User.Email
                                                       })
                                                       .ToListAsync(cancellationToken);
            if (request.IncludeRequestedEvents)
            {
                if (_authenticatedUserId.UserId.HasValue ||
                    _authenticatedUser != AuthenticatedUser.None)
                {
                    var requestedEvents = await _accessRequests.WhereIf(_authenticatedUserId.UserId.HasValue,
                                                                        req => req.UserId_Requestor == _authenticatedUserId.UserId.Value)
                                                               .WhereIf(!_authenticatedUserId.UserId.HasValue && _authenticatedUser != AuthenticatedUser.None,
                                                                        req => req.IdentityProvider == _authenticatedUser.IdentityProvider
                                                                            && req.Identifier == _authenticatedUser.IdentityProviderUserIdentifier)
                                                               .Select(req => new UserInEventDisplayItem
                                                               {
                                                                   EventId = req.EventId,
                                                                   EventName = req.Event.Name,
                                                                   EventAcronym = req.Event.Acronym,
                                                                   EventState = req.Event.State,
                                                                   UserFirstName = req.FirstName,
                                                                   UserLastName = req.LastName,
                                                                   UserEmail = req.Email,
                                                                   RequestSent = true
                                                               })
                                                               .ToListAsync(cancellationToken);
                    authorizedEvents.AddRange(requestedEvents);
                }
            }

            return authorizedEvents;
        }
    }
}