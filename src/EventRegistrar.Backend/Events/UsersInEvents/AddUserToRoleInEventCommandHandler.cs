using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class AddUserToRoleInEventCommandHandler : IRequestHandler<AddUserToRoleInEventCommand, Guid>
{
    private readonly IRepository<UserInEvent> _usersInEvents;

    public AddUserToRoleInEventCommandHandler(IRepository<UserInEvent> usersInEvents)
    {
        _usersInEvents = usersInEvents;
    }

    public async Task<Guid> Handle(AddUserToRoleInEventCommand command, CancellationToken cancellationToken)
    {
        var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == command.EventId
                                                                       && uie.UserId == command.UserId
                                                                       && uie.Role == command.Role, cancellationToken);
        if (userInEvent == null)
        {
            userInEvent = new UserInEvent
                          {
                              Id = Guid.NewGuid(),
                              EventId = command.EventId,
                              UserId = command.UserId,
                              Role = command.Role
                          };
            await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
        }

        return userInEvent.Id;
    }
}