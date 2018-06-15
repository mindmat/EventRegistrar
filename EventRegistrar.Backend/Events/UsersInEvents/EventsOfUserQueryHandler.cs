using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class EventsOfUserQueryHandler : IRequestHandler<EventsOfUserQuery, IEnumerable<UserInEventDisplayItem>>
    {
        private readonly AuthenticatedUser _authenticatedUser;
        private readonly IQueryable<UserInEvent> _usersInEvents;

        public EventsOfUserQueryHandler(IQueryable<UserInEvent> usersInEvents,
                                        AuthenticatedUser authenticatedUser)
        {
            _usersInEvents = usersInEvents;
            _authenticatedUser = authenticatedUser;
        }

        public async Task<IEnumerable<UserInEventDisplayItem>> Handle(EventsOfUserQuery request, CancellationToken cancellationToken)
        {
            return await _usersInEvents.Where(uie => uie.UserId == _authenticatedUser.UserId)
                                       .Select(uie => new UserInEventDisplayItem
                                       {
                                           EventName = uie.Event.Name,
                                           EventAcronym = uie.Event.Acronym,
                                           EventState = uie.Event.State,
                                           Role = uie.Role,
                                           UserFirstName = uie.User.FirstName,
                                           UserLastName = uie.User.LastName,
                                           UserIdentifier = uie.User.IdentityProviderUserIdentifier
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}