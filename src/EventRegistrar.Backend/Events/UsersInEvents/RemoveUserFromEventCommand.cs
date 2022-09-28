using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class RemoveUserFromEventCommand : IRequest<Unit>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
}

public class RemoveUserFromEventCommandHandler : IRequestHandler<RemoveUserFromEventCommand>
{
    private readonly IRepository<UserInEvent> _usersInEvents;
    private readonly IEventBus _eventBus;

    public RemoveUserFromEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                             IEventBus eventBus)
    {
        _usersInEvents = usersInEvents;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RemoveUserFromEventCommand command, CancellationToken cancellationToken)
    {
        var userInEvent = await _usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == command.EventId
                                                                       && uie.UserId == command.UserId, cancellationToken);

        if (userInEvent != null)
        {
            _usersInEvents.Remove(userInEvent);
        }

        _eventBus.Publish(new ReadModelUpdated
                          {
                              EventId = command.EventId,
                              QueryName = nameof(UsersOfEventQuery)
                          });

        return Unit.Value;
    }
}