using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Calendar
{
    public class RegistrableIcs : Entity
    {
        public Guid RegistrableId { get; set; }
        public Registrable? Registrable { get; set; }
        public string? Title { get; set; }
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
                   .WithMany(rbl => rbl.Ics)
                   .HasForeignKey(rbc => rbc.RegistrableId);

            builder.Property(rbl => rbl.Title)
                   .HasMaxLength(1000);
            builder.Property(rbl => rbl.Location)
                   .HasMaxLength(500);
        }
    }
}