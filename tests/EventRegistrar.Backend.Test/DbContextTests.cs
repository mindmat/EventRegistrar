using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;

using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class DbContextTests
    {
        [Fact]
        public async Task SimpleQuery()
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            var connectionString = "Server=tcp:eventregistrator.database.windows.net,1433;Initial Catalog=EventRegistrator;Persist Security Info=False;User ID=eventregistrator;Password={0};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            optionsBuilder.UseSqlServer(connectionString, builder =>
            {
                builder.EnableRetryOnFailure();
            });
            optionsBuilder.EnableSensitiveDataLogging();

            var context = new EventRegistratorDbContext(optionsBuilder.Options);
            var set = context.Set<User>();
            var all = await set.ToListAsync();
        }
    }
}
