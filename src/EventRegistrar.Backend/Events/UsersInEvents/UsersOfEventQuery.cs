using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UsersOfEventQuery : IRequest<IEnumerable<UserInEventDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class UserInEventDisplayItem
{
    public Guid EventId { get; set; }
    public UserInEventRole Role { get; set; }
    public string RoleText { get; set; } = null!;
    public string? UserEmail { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;
    public string? UserAvatarUrl { get; set; }
}

public class UsersOfEventQueryHandler : IRequestHandler<UsersOfEventQuery, IEnumerable<UserInEventDisplayItem>>
{
    private readonly IQueryable<UserInEvent> _usersInEvents;
    private readonly EnumTranslator _enumTranslator;

    public UsersOfEventQueryHandler(IQueryable<UserInEvent> usersInEvents,
                                    EnumTranslator enumTranslator)
    {
        _usersInEvents = usersInEvents;
        _enumTranslator = enumTranslator;
    }

    public async Task<IEnumerable<UserInEventDisplayItem>> Handle(UsersOfEventQuery query,
                                                                  CancellationToken cancellationToken)
    {
        return await _usersInEvents.Where(uie => uie.EventId == query.EventId)
                                   .Select(uie => new UserInEventDisplayItem
                                                  {
                                                      EventId = uie.EventId,
                                                      UserId = uie.UserId,
                                                      Role = uie.Role,
                                                      RoleText = _enumTranslator.Translate(uie.Role),
                                                      UserDisplayName = $"{uie.User!.FirstName} {uie.User.LastName}",
                                                      UserEmail = uie.User.Email,
                                                      UserAvatarUrl = uie.User.AvatarUrl
                                                  })
                                   .ToListAsync(cancellationToken);
    }
}