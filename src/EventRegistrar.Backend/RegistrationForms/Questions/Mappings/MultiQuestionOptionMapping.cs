using EventRegistrar.Backend.RegistrationForms.FormPaths;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class MultiQuestionOptionMapping : Entity
{
    public Guid RegistrationFormId { get; set; }
    public RegistrationForm? RegistrationForm { get; set; }
    public ICollection<Guid> QuestionOptionIds { get; set; } = null!;
    public ICollection<string> RegistrableCombinedIds { get; set; } = null!;
    public int SortKey { get; set; }
}

public class MultiQuestionOptionMappingMap : EntityMap<MultiQuestionOptionMapping>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MultiQuestionOptionMapping> builder)
    {
        builder.ToTable("MultiQuestionOptionMappings");

        builder.HasOne(mqm => mqm.RegistrationForm)
               .WithMany(frm => frm.MultiMappings)
               .HasForeignKey(mqm => mqm.RegistrationFormId);

        builder.Property(qop => qop.QuestionOptionIds)
               .IsCsvColumn();
        builder.Property(qop => qop.RegistrableCombinedIds)!
               .IsJsonColumn();
    }
}