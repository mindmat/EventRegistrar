using Microsoft.EntityFrameworkCore.Design;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class EventRegistratorDbContextFactory : IDesignTimeDbContextFactory<EventRegistratorDbContext>
{
    public EventRegistratorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=.\\SQL2016STD;Database=EventRegistrator_Dev;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new EventRegistratorDbContext(optionsBuilder.Options);
    }
}