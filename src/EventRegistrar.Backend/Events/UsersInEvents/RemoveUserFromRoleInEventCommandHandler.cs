using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class RemoveUserFromRoleInEventCommandHandler : IRequestHandler<RemoveUserFromRoleInEventCommand>
    {
        private readonly IRepository<UserInEvent> _usersInEvents;

        public RemoveUserFromRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents)
        {
            _usersInEvents = usersInEvents;
        }

        public async Task<Unit> Handle(RemoveUserFromRoleInEventCommand command, CancellationToken cancellationToken)
        {
            var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == command.EventId
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