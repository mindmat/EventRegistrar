using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Authentication.Users;

public class User : Entity
{
    public ICollection<UserInEvent>? Events { get; set; }

    public IdentityProvider IdentityProvider { get; set; }
    public string? IdentityProviderUserIdentifier { get; set; }

    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class UserMap : EntityMap<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
    }
}