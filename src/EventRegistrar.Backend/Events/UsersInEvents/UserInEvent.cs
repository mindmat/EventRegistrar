using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Authentication.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UserInEvent : Entity
{
    public Event? Event { get; set; }
    public Guid EventId { get; set; }
    public User? User { get; set; }
    public Guid UserId { get; set; }

    public UserInEventRole Role { get; set; }
}

public class UserInEventMap : EntityMap<UserInEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserInEvent> builder)
    {
        builder.ToTable("UsersInEvents");

        builder.HasOne(uie => uie.Event)
               .WithMany(evt => evt.Users)
               .HasForeignKey(uie => uie.EventId);

        builder.HasOne(uie => uie.User)
               .WithMany(evt => evt.Events)
               .HasForeignKey(uie => uie.UserId);
    }
}