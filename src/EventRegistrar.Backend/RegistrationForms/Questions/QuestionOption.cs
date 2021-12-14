using Microsoft.EntityFrameworkCore.Metadata.Builders;

using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms.Questions;

public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
    public IEnumerable<Mappings.QuestionOptionMapping>? Mappings { get; set; }

    public string Answer { get; set; } = null!;
}

public class QuestionOptionMap : EntityMap<QuestionOption>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasOne(qop => qop.Question)
               .WithMany(qst => qst.QuestionOptions)
               .HasForeignKey(qop => qop.QuestionId);
    }
}