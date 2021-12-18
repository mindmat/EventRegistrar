using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public abstract class Entity
{
    public Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

public abstract class EntityMap<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    private const string Sequence = "Sequence";

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(ent => ent.Id)
               .IsClustered(false);

        ConfigureEntity(builder);

        var propertyBuilder = builder.Property<int>(Sequence)
                                     .UseIdentityColumn();
        propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(Sequence)
               .IsUnique()
               .IsClustered();

        builder.Property(ent => ent.RowVersion)
               .IsRowVersion();
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}