using Microsoft.EntityFrameworkCore.Metadata.Builders;

using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.RegistrationForms.Questions;

namespace EventRegistrar.Backend.RegistrationForms;

public class RegistrationForm : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public ICollection<Question>? Questions { get; set; }
    public ICollection<FormPath>? FormPaths { get; set; }

    public string ExternalIdentifier { get; set; } = null!;
    public State State { get; set; }
    public string? Title { get; set; }
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