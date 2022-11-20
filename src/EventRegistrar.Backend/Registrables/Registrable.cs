using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables.Compositions;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Spots;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables;

public class Registrable : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public ICollection<RegistrableComposition>? Compositions { get; set; }
    public ICollection<QuestionOptionMapping>? QuestionOptionMappings { get; set; }
    public ICollection<Reduction>? Reductions { get; set; }
    public ICollection<Seat>? Spots { get; set; }

    public string Name { get; set; } = null!;
    public string? NameSecondary { get; set; } = null!;
    public bool HasWaitingList { get; set; }
    public bool AutomaticPromotionFromWaitingList { get; set; }
    public bool IsCore { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public int? MaximumDoubleSeats { get; set; }
    public int? MaximumSingleSeats { get; set; }

    [Obsolete("Use PricePackage instead")]
    public decimal? Price { get; set; }

    [Obsolete("Use PricePackage instead")]
    public decimal? ReducedPrice { get; set; }

    public int? ShowInMailListOrder { get; set; }
    public string? CheckinListColumn { get; set; }
    public string? Tag { get; set; }
    public RegistrableType Type { get; set; }
}

public enum RegistrableType
{
    Single = 1,
    Double = 2
}

public class RegistrableMap : EntityMap<Registrable>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Registrable> builder)
    {
        builder.ToTable("Registrables");

        builder.HasOne(rbl => rbl.Event)
               .WithMany(evt => evt.Registrables)
               .HasForeignKey(rbl => rbl.EventId);

        builder.Property(rbl => rbl.Name)
               .HasMaxLength(200);

        builder.Property(rbl => rbl.NameSecondary)
               .HasMaxLength(200);

        builder.Property(rbl => rbl.CheckinListColumn)
               .HasMaxLength(200);

        builder.Property(rbl => rbl.Tag)
               .HasMaxLength(200);

        builder.Property(rbl => rbl.Type)
               .HasDefaultValue(RegistrableType.Single);
    }
}