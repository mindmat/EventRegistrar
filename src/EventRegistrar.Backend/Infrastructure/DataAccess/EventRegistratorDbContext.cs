namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class EventRegistratorDbContext(DbContextOptions<EventRegistratorDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(EventRegistratorDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
                            .HavePrecision(18, 2);
    }
}