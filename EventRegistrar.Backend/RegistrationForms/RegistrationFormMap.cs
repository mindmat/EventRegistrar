using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationFormMap : EntityTypeConfiguration<RegistrationForm>
    {
        public override void Configure(EntityTypeBuilder<RegistrationForm> builder)
        {
            base.Configure(builder);
            builder.ToTable("RegistrationForms");

            builder.HasOne(frm => frm.Event)
                   .WithMany()
                   .HasForeignKey(frm => frm.EventId);
        }
    }
}