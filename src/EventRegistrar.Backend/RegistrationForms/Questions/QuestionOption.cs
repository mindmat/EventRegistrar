using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions;

public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }

    public string Answer { get; set; } = null!;
    public IEnumerable<Mappings.QuestionOptionMapping>? Mappings { get; set; }
}

public class QuestionOptionMap : EntityMap<QuestionOption>
{
    public override void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        base.Configure(builder);
        builder.ToTable("QuestionOptions");

        builder.HasOne(qop => qop.Question)
               .WithMany(qst => qst!.QuestionOptions)
               .HasForeignKey(qop => qop.QuestionId);
    }
}