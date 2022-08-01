using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ReadModel<TContent>
    where TContent : class
{
    public TContent Content { get; set; } = null!;
}

public abstract class ReadModelMap<TEntity, TContent> : IEntityTypeConfiguration<TEntity>
    where TEntity : ReadModel<TContent>
    where TContent : class
{
    private const string Sequence = "Sequence";

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureEntity(builder);

        var propertyBuilder = builder.Property<int>(Sequence)
                                     .UseIdentityColumn();
        propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(Sequence)
               .IsUnique()
               .IsClustered();

        builder.Property(rdm => rdm.Content)
               .HasConversion(JsonConverter.Converter<TContent>());
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}