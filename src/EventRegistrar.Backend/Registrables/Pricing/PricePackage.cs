using System.Diagnostics;

using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Pricing;

[DebuggerDisplay("{Name,nq}")]
public class PricePackage : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public ICollection<PricePackagePart>? Parts { get; set; }

    [Obsolete("Use AllowAsAutomaticFallback")]
    public bool AllowAsFallback { get; set; }

    public bool AllowAsAutomaticFallback { get; set; }
    public bool AllowAsManualFallback { get; set; }
    public bool IsCorePackage { get; set; }
    public int FallbackPriority { get; set; }
    public int SortKey { get; set; }
    public bool ShowInOverview { get; set; }
}

public class PricePackageMap : EntityMap<PricePackage>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PricePackage> builder)
    {
        builder.ToTable("PricePackages");

        builder.Property(ppg => ppg.Name)
               .HasMaxLength(500);

        builder.HasOne(ppg => ppg.Event)
               .WithMany()
               .HasForeignKey(ppg => ppg.EventId);
    }
}

public class PricePackagePart : Entity
{
    public Guid PricePackageId { get; set; }
    public PricePackage? PricePackage { get; set; }
    public ICollection<RegistrableInPricePackagePart>? Registrables { get; set; }
    public PricePackagePartSelectionType SelectionType { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public int SortKey { get; set; }
    public bool ShowInMailSpotList { get; set; }
}

public enum PricePackagePartSelectionType
{
    AnyOne = 1,
    AnyTwo = 2,
    AnyThree = 3,
    All = 10,
    Optional = 11
}

public class PricePackagePartMap : EntityMap<PricePackagePart>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PricePackagePart> builder)
    {
        builder.ToTable("PricePackagePart");

        builder.HasOne(ppp => ppp.PricePackage)
               .WithMany(ppg => ppg.Parts)
               .HasForeignKey(ppp => ppp.PricePackageId);
    }
}

public class RegistrableInPricePackagePart : Entity
{
    public Guid RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }
    public Guid PricePackagePartId { get; set; }
    public PricePackagePart? PricePackagePart { get; set; }
}

public class RegistrableInPricePackagePartMap : EntityMap<RegistrableInPricePackagePart>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrableInPricePackagePart> builder)
    {
        builder.ToTable("RegistrableInPricePackageParts");

        builder.HasOne(rip => rip.Registrable)
               .WithMany()
               .HasForeignKey(rip => rip.RegistrableId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rip => rip.PricePackagePart)
               .WithMany(ppp => ppp.Registrables)
               .HasForeignKey(rip => rip.PricePackagePartId);
    }
}