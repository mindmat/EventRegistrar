using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public class DomainEventMap : IEntityTypeConfiguration<DomainEvent>
    {
        public void Configure(EntityTypeBuilder<DomainEvent> builder)
        {
            builder.ToTable("DomainEvents");
            builder.HasKey(ent => ent.Id);
            //builder.Property(evt => evt.Sequence)
            //       .UseSqlServerIdentityColumn();
        }
    }
}