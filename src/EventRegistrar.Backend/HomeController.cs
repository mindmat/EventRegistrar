using System.Security.Claims;

using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend;

public class HomeController : Controller
{
    private readonly EventRegistratorDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;

    public HomeController(
        EventRegistratorDbContext dbContext,
        IConfiguration config,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _config = config;
        _environment = environment;
    }

    public async Task<object> Index()
    {
        return new
               {
                   DbModelPendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(),
                   DbModelAppliedMigration = await _dbContext.Database.GetAppliedMigrationsAsync(),
                   FrontendUrl = new Uri(FrontendUrl),
                   AppInsightsKey = _config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"),
                   CorsOrigins = _config.GetValue<string>("CORS_ORIGINS"),
                   _environment.EnvironmentName,
                   Identity = new
                              {
                                  User?.Identity?.IsAuthenticated,
                                  User?.Identity?.Name,
                                  Claims = (User?.Identity as ClaimsIdentity)?.Claims.Select(clm => clm.ToString())
                              }
               };
    }

    private string FrontendUrl => _config.GetValue<string>("FRONTEND_URL") ?? "http://localhost:4200";
}