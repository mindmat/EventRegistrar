using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UsersOfEventQueryHandler : IRequestHandler<UsersOfEventQuery, IEnumerable<UserInEventDisplayItem>>
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IQueryable<UserInEvent> _usersInEvents;

        public UsersOfEventQueryHandler(IEventAcronymResolver eventAcronymResolver,
                                        IQueryable<UserInEvent> usersInEvents)
        {
            _eventAcronymResolver = eventAcronymResolver;
            _usersInEvents = usersInEvents;
        }

        public async Task<IEnumerable<UserInEventDisplayItem>> Handle(UsersOfEventQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _eventAcronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            return await _usersInEvents.Where(uie => uie.EventId == eventId)
                                       .Select(uie => new UserInEventDisplayItem
                                       {
                                           EventName = uie.Event.Name,
                                           EventAcronym = uie.Event.Acronym,
                                           EventState = uie.Event.State,
                                           Role = uie.Role,
                                           UserFirstName = uie.User.FirstName,
                                           UserLastName = uie.User.LastName,
                                           UserEmail = uie.User.Email
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}