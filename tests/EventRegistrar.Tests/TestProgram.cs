using System.Security.Claims;
using System.Text.Encodings.Web;

using ApiTestInfrastructure;

using EventRegistrar.Backend;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Infrastructure.Startup;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SimpleInjector;


var builder = WebApplication.CreateBuilder(args);

var container = new Container();
container.Options.ResolveUnregisteredConcreteTypes = true;
container.Options.DefaultLifestyle = Lifestyle.Scoped;

builder.Services.AddMemoryCache();
builder.Services.AddSimpleInjector(container, options =>
{
    // AddAspNetCore() wraps web requests in a Simple Injector scope and
    // allows request-scoped framework services to be resolved.
    options.AddAspNetCore();

    // Optionally, allow application components to depend on the non-generic
    // ILogger (Microsoft.Extensions.Logging) abstractions.
    options.AddLogging();
});
builder.Services.AddSignalR();
builder.Services.AddSingleton(container);
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddApplicationInsights()
                                                            .SetMinimumLevel(LogLevel.Information));

builder.Services.AddAuthentication("IntegrationTest")
       .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthenticationHandler>(
           "IntegrationTest",
           options => { }
       );
builder.Services.AddAuthorization(o => o.AddPolicy("api", p => p.RequireAuthenticatedUser()));
//builder.Services.AddCors();

//builder.Services.AddSwaggerDocument();
//builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider>(_ => new MediatorEndpointApiDescriptionGroupCollectionProvider(container.GetInstance<RequestRegistry>()));
//builder.Services.AddOpenApiDocument(document => document.DocumentName = "v1");
//builder.Services.AddApplicationInsightsTelemetry(options =>
//{
//    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
//    options.DeveloperMode = builder.Environment.IsDevelopment();
//});
//builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, _) => { module.EnableSqlCommandTextInstrumentation = true; });
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();

((IApplicationBuilder)app).UseSimpleInjector(container);
var assemblies = new[] { typeof(HomeController).Assembly };
container.ResolveUnregisteredType += (sender, e) =>
{
    var type = assemblies.Select(ass => ass.GetType(e.UnregisteredServiceType.Name)).FirstOrDefault(typ => typ != null);
    if (type != null)
    {
        e.Register(() => type);
    }
};
container.RegisterInstance(builder.Configuration);
container.RegisterInstance<IConfiguration>(builder.Configuration);

container.RegisterMediatr(assemblies, true);
container.RegisterDataAccessToInMemoryDb(assemblies);
container.RegisterIamWithAuth0();
container.RegisterEventBus(assemblies);
container.RegisterInMemoryQueue();
container.RegisterErrorHandling(assemblies);
container.RegisterConfiguration(assemblies);
container.RegisterMisc(assemblies);
container.Verify();

app.UseDeveloperExceptionPage();

//app.UseOpenApi();
//app.UseSwaggerUi();

app.UseRequestLocalization();
app.UseMiddleware<ExceptionMiddleware>(container);


//app.UseHttpsRedirection();
app.UseRouting();
//app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
//                                      .AllowAnyHeader()
//                                      .AllowAnyMethod());


app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRequests(container);
    endpoints.MapHub<NotificationHub>("/notifications");
});

app.Services.GetService<TelemetryConfiguration>()?.DisableTelemetry = true;
app.Run();


public partial class TestProgram;

internal class IntegrationTestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public IntegrationTestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                                ILoggerFactory logger,
                                                UrlEncoder encoder,
                                                ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
                     {
                         new Claim(ClaimTypes.Name, "IntegrationTest User"),
                         new Claim(ClaimTypes.NameIdentifier, "IntegrationTest User"),
                         new Claim("a-custom-claim", "squirrel 🐿️"),
                     };
        var identity = new ClaimsIdentity(claims, "IntegrationTest");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "IntegrationTest");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}