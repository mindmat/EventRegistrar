using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class Shift : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public Guid? RegistrableId_ShiftPreference { get; set; }
    public Registrable? Registrable_ShiftPreference { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Number of helper participants needed (excluding the responsible person)
    /// </summary>
    public int HelpersNeeded { get; set; }

    /// <summary>
    /// The participant responsible for this shift
    /// </summary>
    public Guid? RegistrationId_Responsible { get; set; }

    public Registration? Registration_Responsible { get; set; }

    /// <summary>
    /// Helper assignments for this shift
    /// </summary>
    public ICollection<ShiftAssignment>? Assignments { get; set; }
}

public class ShiftMap : EntityMap<Shift>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        builder.HasOne(sft => sft.Event)
               .WithMany()
               .HasForeignKey(sft => sft.EventId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sft => sft.Registrable_ShiftPreference)
               .WithMany()
               .HasForeignKey(sft => sft.RegistrableId_ShiftPreference)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sft => sft.Registration_Responsible)
               .WithMany()
               .HasForeignKey(sft => sft.RegistrationId_Responsible)
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(sft => sft.Name)
               .HasMaxLength(200);

        builder.Property(sft => sft.Description)
               .HasMaxLength(1000);

        builder.Property(sft => sft.Location)
               .HasMaxLength(200);
    }
}