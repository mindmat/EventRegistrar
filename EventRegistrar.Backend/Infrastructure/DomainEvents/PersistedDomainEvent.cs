using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class PersistedDomainEvent
    {
        public string Data { get; set; }
        public Guid? DomainEventId_Parent { get; set; }
        public Guid? EventId { get; set; }
        public Guid Id { get; set; }

        public long Sequence { get; set; }
        public DateTime Timestamp { get; set; }

        public string Type { get; set; }
    }

    public class PersistedDomainEventMap : IEntityTypeConfiguration<PersistedDomainEvent>
    {
        public void Configure(EntityTypeBuilder<PersistedDomainEvent> builder)
        {
            builder.ToTable("DomainEvents");
            builder.HasKey(dev => dev.Id);
            builder.HasIndex(dev => dev.Sequence)
                   .IsClustered();

            builder.Property(dev => dev.Sequence)
                   .ValueGeneratedOnAdd();
            //builder.Property(evt => evt.Sequence)
            //       .UseSqlServerIdentityColumn();
        }
    }
}