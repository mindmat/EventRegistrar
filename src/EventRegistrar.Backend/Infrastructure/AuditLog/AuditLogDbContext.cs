namespace EventRegistrar.Backend.Infrastructure.AuditLog;

public class AuditLogDbContext(DbContextOptions<AuditLogDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new RequestLogMap());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
                            .HavePrecision(18, 2);
    }
}