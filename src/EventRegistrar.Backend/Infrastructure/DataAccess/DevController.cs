using Microsoft.AspNetCore.Mvc;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class DevController(DbContext dbContext, Container container) : Controller
{
    private readonly Container _container = container;
    private readonly DbContext _dbContext = dbContext;

    [HttpPost("api/dev/migratedb")]
    public async Task MigrateDb()
    {
        //await _dbContext.Database.MigrateAsync();
        //var scenario = new TestScenario();
        //await scenario.Create(_container);
        await Task.CompletedTask;
    }
}