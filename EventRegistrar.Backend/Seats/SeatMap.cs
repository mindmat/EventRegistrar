using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Seats
{
    public class SeatMap : EntityTypeConfiguration<Seat>
    {
        public override void Configure(EntityTypeBuilder<Seat> builder)
        {
            base.Configure(builder);
            builder.ToTable("Seats");
            builder.HasOne(seat => seat.Registrable)
                   .WithMany(rbl => rbl.Seats)
                   .HasForeignKey(rbl => rbl.RegistrableId);

            builder.HasOne(seat => seat.Registration)
                   .WithMany(reg => reg.Seats_AsLeader)
                   .HasForeignKey(seat => seat.RegistrationId);

            builder.HasOne(seat => seat.Registration_Follower)
                   .WithMany(reg => reg.Seats_AsFollower)
                   .HasForeignKey(seat => seat.RegistrationId_Follower);
        }
    }
}