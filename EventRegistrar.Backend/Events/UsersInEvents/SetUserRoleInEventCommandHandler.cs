using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class SetUserRoleInEventCommandHandler : IRequestHandler<SetUserRoleInEventCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public SetUserRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                                IEventAcronymResolver acronymResolver)
        {
            _usersInEvents = usersInEvents;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(SetUserRoleInEventCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == eventId
                                                                           && uie.UserId == command.UserId, cancellationToken)
                                 ?? new UserInEvent { EventId = eventId, UserId = command.UserId };
            if (userInEvent.Role != command.Role)
            {
                userInEvent.Role = command.Role;
                await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
            }

            return new Unit();
        }
    }
}