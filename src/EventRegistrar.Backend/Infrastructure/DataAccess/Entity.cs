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
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(ent => ent.Id);

        builder.Property(ent => ent.RowVersion)
               .IsRowVersion();

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}