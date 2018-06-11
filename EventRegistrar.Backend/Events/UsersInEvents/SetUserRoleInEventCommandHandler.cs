using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class SetUserRoleInEventCommand : IRequest<Unit>
    {
        public string EventAcronym { get; set; }
        public UserInEventRole Role { get; set; }
        public Guid UserId { get; set; }
    }

    public class SetUserRoleInEventCommandHandler : IRequestHandler<SetUserRoleInEventCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IAuthorizationChecker _authorizationChecker;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public SetUserRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                                IEventAcronymResolver acronymResolver,
                                                IAuthorizationChecker authorizationChecker)
        {
            _usersInEvents = usersInEvents;
            _acronymResolver = acronymResolver;
            _authorizationChecker = authorizationChecker;
        }

        public async Task<Unit> Handle(SetUserRoleInEventCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            await _authorizationChecker.ThrowIfUserIsNotInEventRole(eventId, UserInEventRole.Admin);

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