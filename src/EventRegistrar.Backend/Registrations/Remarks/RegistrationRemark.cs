using EventRegistrar.Backend.RegistrationForms.Questions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class RegistrationRemark : Entity
{
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public Guid? QuestionId { get; set; }
    public Question? Question { get; set; }
    public string Text { get; set; } = null!;
    public bool Processed { get; set; }
}

public class RegistrationRemarkMap : EntityMap<RegistrationRemark>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrationRemark> builder)
    {
        builder.ToTable("RegistrationRemarks");

        builder.HasOne(rmk => rmk.Registration)
               .WithMany(reg => reg.RemarksList)
               .HasForeignKey(rmk => rmk.RegistrationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rmk => rmk.Question)
               .WithMany()
               .HasForeignKey(rmk => rmk.QuestionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}