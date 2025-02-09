using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Calendar
{
    public class RegistrableIcs : Entity
    {
        public Registrable? Registrable { get; set; }
        public bool AddToCalendar { get; set; }
        public string? Location { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string? ContentHtml { get; set; }

    }

    public class RegistrableIcsMap : EntityMap<RegistrableIcs>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<RegistrableIcs> builder)
        {
            builder.ToTable("RegistrablesIcs");

            builder.HasOne(rbc => rbc.Registrable)
                   .WithOne(rbl => rbl.Ics)
                   .HasForeignKey<RegistrableIcs>(rbc => rbc.Id);

            builder.Property(rbl => rbl.Location)
                   .HasMaxLength(500);
        }
    }
}