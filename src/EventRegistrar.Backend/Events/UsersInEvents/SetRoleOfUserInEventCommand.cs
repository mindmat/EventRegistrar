using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class SetRoleOfUserInEventCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public UserInEventRole Role { get; set; }
}

public class SetRoleOfUserInEventCommandHandler(IRepository<UserInEvent> usersInEvents,
                                                IEventBus eventBus)
    : IRequestHandler<SetRoleOfUserInEventCommand>
{
    public async Task Handle(SetRoleOfUserInEventCommand command, CancellationToken cancellationToken)
    {
        var userInEvent = await usersInEvents.AsTracking()
                                             .FirstOrDefaultAsync(uie => uie.EventId == command.EventId
                                                                      && uie.UserId == command.UserId, cancellationToken);
        if (userInEvent == null)
        {
            usersInEvents.InsertObjectTree(new UserInEvent
                                           {
                                               Id = Guid.NewGuid(),
                                               EventId = command.EventId,
                                               UserId = command.UserId
                                           });
        }
        else
        {
            userInEvent.Role = command.Role;
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(UsersOfEventQuery)
                         });
    }
}