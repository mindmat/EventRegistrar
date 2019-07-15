using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionOptionToRegistrableMappingMap : EntityTypeConfiguration<QuestionOptionToRegistrableMapping>
    {
        public override void Configure(EntityTypeBuilder<QuestionOptionToRegistrableMapping> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuestionOptionToRegistrableMappings");

            builder.HasOne(qop => qop.Registrable)
                   .WithMany(qst => qst.QuestionOptionMappings)
                   .HasForeignKey(qop => qop.RegistrableId);

            builder.HasOne(qop => qop.QuestionOption)
                   .WithMany(qst => qst.Registrables)
                   .HasForeignKey(qop => qop.QuestionOptionId);
        }
    }
}