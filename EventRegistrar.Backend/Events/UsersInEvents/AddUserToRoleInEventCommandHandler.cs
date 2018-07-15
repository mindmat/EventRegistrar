using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AddUserToRoleInEventCommandHandler : IRequestHandler<AddUserToRoleInEventCommand, Guid>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public AddUserToRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                                  IEventAcronymResolver acronymResolver)
        {
            _usersInEvents = usersInEvents;
            _acronymResolver = acronymResolver;
        }

        public async Task<Guid> Handle(AddUserToRoleInEventCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == eventId
                                                                           && uie.UserId == command.UserId
                                                                           && uie.Role == command.Role, cancellationToken);
            if (userInEvent == null)
            {
                userInEvent = new UserInEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = command.UserId,
                    Role = command.Role
                };
                await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
            }

            return userInEvent.Id;
        }
    }
}