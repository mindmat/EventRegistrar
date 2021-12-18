using Microsoft.EntityFrameworkCore.Design;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class EventRegistratorDbContextFactory : IDesignTimeDbContextFactory<EventRegistratorDbContext>
{
    public EventRegistratorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>()
                             .UseSqlServer("Server=.\\SQL2016STD;Database=Test;Trusted_Connection=true;Encrypt=True;TrustServerCertificate=true")
                             .EnableSensitiveDataLogging();

        return new EventRegistratorDbContext(optionsBuilder.Options);
    }
}