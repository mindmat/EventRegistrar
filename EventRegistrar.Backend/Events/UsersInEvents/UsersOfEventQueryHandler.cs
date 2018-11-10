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
        private readonly IQueryable<UserInEvent> _usersInEvents;

        public UsersOfEventQueryHandler(IQueryable<UserInEvent> usersInEvents)
        {
            _usersInEvents = usersInEvents;
        }

        public async Task<IEnumerable<UserInEventDisplayItem>> Handle(UsersOfEventQuery query, CancellationToken cancellationToken)
        {
            return await _usersInEvents.Where(uie => uie.EventId == query.EventId)
                                       .Select(uie => new UserInEventDisplayItem
                                       {
                                           EventName = uie.Event.Name,
                                           EventAcronym = uie.Event.Acronym,
                                           EventState = uie.Event.State,
                                           Role = uie.Role,
                                           UserId = uie.UserId,
                                           UserFirstName = uie.User.FirstName,
                                           UserLastName = uie.User.LastName,
                                           UserEmail = uie.User.Email
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}