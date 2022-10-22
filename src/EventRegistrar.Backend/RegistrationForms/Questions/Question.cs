using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions;

public class Question : Entity
{
    public Guid RegistrationFormId { get; set; }
    public RegistrationForm? RegistrationForm { get; set; }
    public ICollection<QuestionOption>? QuestionOptions { get; set; }

    public int ExternalId { get; set; }
    public int Index { get; set; }

    public QuestionType Type { get; set; }
    public string Title { get; set; } = null!;
    public string? Section { get; set; }

    public QuestionMappingType? Mapping { get; set; }
    public string? TemplateKey { get; set; }
}

public class QuestionMap : EntityMap<Question>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasOne(que => que.RegistrationForm)
               .WithMany(frm => frm.Questions)
               .HasForeignKey(que => que.RegistrationFormId);
    }
}

public enum QuestionMappingType
{
    FirstName = 1,
    LastName = 2,
    EMail = 3,
    Phone = 4,
    Town = 5,
    Remarks = 6,
    Iban = 7,
    Partner = 10
}