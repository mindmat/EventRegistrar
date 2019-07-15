using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        public override void Configure(EntityTypeBuilder<Question> builder)
        {
            base.Configure(builder);
            builder.ToTable("Questions");

            builder.HasOne(que => que.RegistrationForm)
                   .WithMany()
                   .HasForeignKey(que => que.RegistrationFormId);
        }
    }
}