using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class SetRoleOfUserInEventCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public UserInEventRole Role { get; set; }
}

public class SetRoleOfUserInEventCommandHandler : IRequestHandler<SetRoleOfUserInEventCommand>
{
    private readonly IRepository<UserInEvent> _usersInEvents;
    private readonly IEventBus _eventBus;

    public SetRoleOfUserInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                              IEventBus eventBus)
    {
        _usersInEvents = usersInEvents;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(SetRoleOfUserInEventCommand command, CancellationToken cancellationToken)
    {
        var userInEvent = await _usersInEvents.AsTracking()
                                              .FirstOrDefaultAsync(uie => uie.EventId == command.EventId
                                                                       && uie.UserId == command.UserId, cancellationToken);
        if (userInEvent == null)
        {
            userInEvent = new UserInEvent
                          {
                              Id = Guid.NewGuid(),
                              EventId = command.EventId,
                              UserId = command.UserId
                          };
            await _usersInEvents.InsertOrUpdateEntity(userInEvent, cancellationToken);
        }
        else
        {
            userInEvent.Role = command.Role;
        }

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(UsersOfEventQuery)
                          });

        return Unit.Value;
    }
}