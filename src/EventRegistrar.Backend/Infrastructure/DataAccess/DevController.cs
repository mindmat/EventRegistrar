using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class DevController : Controller
    {
        private readonly Container _container;
        private readonly DbContext _dbContext;

        public DevController(DbContext dbContext, Container container)
        {
            _dbContext = dbContext;
            _container = container;
        }

        [HttpPost("api/dev/migratedb")]
        public async Task MigrateDb()
        {
            //await _dbContext.Database.MigrateAsync();
            //var scenario = new TestScenario();
            //await scenario.Create(_container);
            await Task.CompletedTask;
        }
    }
}