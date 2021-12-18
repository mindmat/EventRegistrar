namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class EventRegistratorDbContext : DbContext
{
    public EventRegistratorDbContext(DbContextOptions<EventRegistratorDbContext> options)
        : base(options)
    {
    }

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