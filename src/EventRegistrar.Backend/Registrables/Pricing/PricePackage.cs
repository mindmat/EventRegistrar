using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Pricing;

public class PricePackage : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public ICollection<PricePackagePart>? Parts { get; set; }
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
    public decimal? Reduction { get; set; }
    public bool IsOptional { get; set; }
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