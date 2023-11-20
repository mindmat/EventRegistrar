using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend;

public class HomeController(EventRegistratorDbContext dbContext,
                            IConfiguration config,
                            IWebHostEnvironment environment)
    : Controller
{
    public async Task<object> Index()
    {
        return new
               {
                   DbModelPendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(),
                   DbModelAppliedMigration = await dbContext.Database.GetAppliedMigrationsAsync(),
                   FrontendUrl = new Uri(FrontendUrl),
                   AppInsightsKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY"),
                   CorsOrigins = config.GetValue<string>("CORS_ORIGINS"),
                   environment.EnvironmentName,
                   Identity = new
                              {
                                  User?.Identity?.IsAuthenticated,
                                  User?.Identity?.Name,
                                  Claims = (User?.Identity as ClaimsIdentity)?.Claims.Select(clm => clm.ToString())
                              }
               };
    }

    private string FrontendUrl => config.GetValue<string>("FRONTEND_URL") ?? "http://localhost:4200";
}