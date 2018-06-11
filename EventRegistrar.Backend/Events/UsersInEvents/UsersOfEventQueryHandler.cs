using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UsersOfEventQueryHandler : IRequestHandler<UsersOfEventQuery, IEnumerable<UserInEventDisplayItem>>
    {
        private readonly IAuthorizationChecker _authorizationChecker;
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IQueryable<UserInEvent> _usersInEvents;

        public UsersOfEventQueryHandler(IEventAcronymResolver eventAcronymResolver,
                                        IAuthorizationChecker authorizationChecker,
                                        IQueryable<UserInEvent> usersInEvents)
        {
            _eventAcronymResolver = eventAcronymResolver;
            _authorizationChecker = authorizationChecker;
            _usersInEvents = usersInEvents;
        }

        public async Task<IEnumerable<UserInEventDisplayItem>> Handle(UsersOfEventQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _eventAcronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            await _authorizationChecker.ThrowIfUserIsNotInEventRole(eventId, UserInEventRole.Admin);

            return await _usersInEvents.Where(uie => uie.EventId == eventId)
                                       .Select(uie => new UserInEventDisplayItem
                                       {
                                           EventName = uie.Event.Name,
                                           EventAcronym = uie.Event.Acronym,
                                           EventState = uie.Event.State,
                                           Role = uie.Role,
                                           UserName = uie.User.Name,
                                           UserIdentifier = uie.User.IdentityProviderUserIdentifier
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}