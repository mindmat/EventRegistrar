using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        public override void Configure(EntityTypeBuilder<Question> builder)
        {
            base.Configure(builder);
            builder.ToTable("Questions");
        }
    }
}