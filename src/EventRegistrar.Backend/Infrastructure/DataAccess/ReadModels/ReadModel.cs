using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ReadModel
{
    public string QueryName { get; set; } = null!;
    public Guid EventId { get; set; }
    public Guid? RowId { get; set; }
    public string ContentJson { get; set; } = null!;
    public DateTimeOffset LastUpdate { get; set; }
}

public class ReadModelMap : IEntityTypeConfiguration<ReadModel>
{
    private const string Sequence = "Sequence";

    public void Configure(EntityTypeBuilder<ReadModel> builder)
    {
        builder.ToTable("ReadModels");

        var propertyBuilder = builder.Property<int>(Sequence)
                                     .UseIdentityColumn();
        propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasKey(Sequence)
               .IsClustered();

        builder.HasIndex(rdm => new
                                {
                                    rdm.QueryName,
                                    rdm.EventId,
                                    rdm.RowId
                                })
               .IsUnique();
    }
}