using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class RemoveUserFromEventCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
}

public class RemoveUserFromEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                               IEventBus eventBus)
    : IRequestHandler<RemoveUserFromEventCommand>
{
    public async Task Handle(RemoveUserFromEventCommand command, CancellationToken cancellationToken)
    {
        var userInEvent = await usersInEvents.FirstOrDefaultAsync(uie => uie.EventId == command.EventId
                                                                      && uie.UserId == command.UserId, cancellationToken);

        if (userInEvent != null)
        {
            usersInEvents.Remove(userInEvent);
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(UsersOfEventQuery)
                         });
    }
}