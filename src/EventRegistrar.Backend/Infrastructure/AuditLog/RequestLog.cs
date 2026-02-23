using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.AuditLog;

public class RequestLog
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public string RequestType { get; set; } = null!;
    public string RequestJson { get; set; } = null!;
    public Guid? UserId { get; set; }
    public DateTimeOffset When { get; set; }
    public string? UserDisplayText { get; set; }
    public string? Exception { get; set; }
    public long ExecutionTimeInMilliseconds { get; set; }
}

public class RequestLogMap : IEntityTypeConfiguration<RequestLog>
{
    public const string SequencePropertyName = "Sequence";

    public void Configure(EntityTypeBuilder<RequestLog> builder)
    {
        builder.ToTable(nameof(RequestLog));

        builder.Property(req => req.Id)
               .HasDefaultValueSql("NEWID()");

        builder.HasKey(req => req.Id)
               .IsClustered(false);

        builder.Property<long>(SequencePropertyName)
               .UseIdentityColumn()
               .ValueGeneratedOnAdd();
        //propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        //propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(SequencePropertyName)
               .IsUnique()
               .IsClustered();

        builder.Property(req => req.UserDisplayText)
               .HasMaxLength(500);

        builder.Property(req => req.RequestType)
               .HasMaxLength(500);

        builder.HasIndex(req => new { req.EventId, req.RequestType });
    }
}