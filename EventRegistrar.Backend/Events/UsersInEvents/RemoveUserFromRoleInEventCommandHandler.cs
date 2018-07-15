using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class RemoveUserFromRoleInEventCommandHandler : IRequestHandler<RemoveUserFromRoleInEventCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public RemoveUserFromRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                                       IEventAcronymResolver acronymResolver)
        {
            _usersInEvents = usersInEvents;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(RemoveUserFromRoleInEventCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == eventId
                                                                           && uie.UserId == command.UserId
                                                                           && uie.Role == command.Role, cancellationToken);

            if (userInEvent != null)
            {
                _usersInEvents.Remove(userInEvent);
            }

            return new Unit();
        }
    }
}