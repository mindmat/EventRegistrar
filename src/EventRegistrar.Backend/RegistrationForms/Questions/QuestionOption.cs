using EventRegistrar.Backend.Registrations.Responses;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions;

public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
    public IEnumerable<Mappings.QuestionOptionMapping>? Mappings { get; set; }

    public string Answer { get; set; } = null!;
    public ICollection<Response>? Responses { get; set; }
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