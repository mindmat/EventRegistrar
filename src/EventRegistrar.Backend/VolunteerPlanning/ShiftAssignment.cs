using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class ShiftAssignment : Entity
{
    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
}

public class ShiftAssignmentMap : EntityMap<ShiftAssignment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.ToTable("ShiftAssignments");

        builder.HasOne(sas => sas.Shift)
               .WithMany(sft => sft.Assignments)
               .HasForeignKey(sas => sas.ShiftId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sas => sas.Registration)
               .WithMany()
               .HasForeignKey(sas => sas.RegistrationId)
               .OnDelete(DeleteBehavior.Restrict);

        // Ensure one registration can only be assigned once to the same shift
        builder.HasIndex(sas => new { sas.ShiftId, sas.RegistrationId })
               .IsUnique();
    }
}