using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms;

public class RegistrationForm : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public ICollection<Question>? Questions { get; set; }
    public ICollection<FormPath>? FormPaths { get; set; }

    public string ExternalIdentifier { get; set; } = null!;
    public EventState State { get; set; }
    public string? Title { get; set; }
    public ICollection<MultiQuestionOptionMapping>? MultiMappings { get; set; }
}

public class RegistrationFormMap : EntityMap<RegistrationForm>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrationForm> builder)
    {
        builder.ToTable("RegistrationForms");

        builder.HasOne(frm => frm.Event)
               .WithMany()
               .HasForeignKey(frm => frm.EventId);
    }
}