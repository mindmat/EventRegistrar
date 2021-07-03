using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEventMap : EntityTypeConfiguration<UserInEvent>
    {
        public override void Configure(EntityTypeBuilder<UserInEvent> builder)
        {
            base.Configure(builder);
            builder.ToTable("UsersInEvents");

            builder.HasOne(uie => uie.Event)
                   .WithMany(evt => evt.Users)
                   .HasForeignKey(uie => uie.EventId);

            builder.HasOne(uie => uie.User)
                   .WithMany(evt => evt.Events)
                   .HasForeignKey(uie => uie.UserId);
        }
    }
}