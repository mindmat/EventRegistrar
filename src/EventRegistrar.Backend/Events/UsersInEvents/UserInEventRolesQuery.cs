using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UserInEventRolesQuery : IRequest<IEnumerable<RoleDescription>> { }

public class RoleDescription
{
    public UserInEventRole Role { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class UserInEventRolesQueryHandler : IRequestHandler<UserInEventRolesQuery, IEnumerable<RoleDescription>>
{
    private readonly EnumTranslator _enumTranslator;

    public UserInEventRolesQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<RoleDescription>> Handle(UserInEventRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = new[] { UserInEventRole.Reader, UserInEventRole.Writer, UserInEventRole.Admin }
            .Select(rol => new RoleDescription
                           {
                               Role = rol,
                               Name = _enumTranslator.Translate(rol),
                               Description = GetDescription(rol)
                           });
        return Task.FromResult(roles);
    }

    private static string GetDescription(UserInEventRole role)
    {
        var key = $"{nameof(UserInEventRole)}_{role}_Description";
        return Resources.ResourceManager.GetString(key) ?? role.ToString();
    }
}