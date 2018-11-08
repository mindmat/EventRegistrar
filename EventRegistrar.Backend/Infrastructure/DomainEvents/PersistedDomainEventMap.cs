using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class PersistedDomainEventMap : IEntityTypeConfiguration<PersistedDomainEvent>
    {
        public void Configure(EntityTypeBuilder<PersistedDomainEvent> builder)
        {
            builder.ToTable("DomainEvents");
            builder.HasKey(ent => ent.Id);
            //builder.Property(evt => evt.Sequence)
            //       .UseSqlServerIdentityColumn();
        }
    }
}