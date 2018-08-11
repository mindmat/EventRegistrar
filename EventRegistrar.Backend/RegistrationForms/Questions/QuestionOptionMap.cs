using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionOptionMap : EntityTypeConfiguration<QuestionOption>
    {
        public override void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuestionOptions");

            builder.HasOne(qop => qop.Question)
                   .WithMany(qst => qst.QuestionOptions)
                   .HasForeignKey(qop => qop.QuestionId);
        }
    }
}