using EventRegistrar.Backend;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Infrastructure.Startup;
using EventRegistrar.Backend.Payments.Files;

using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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

builder.Services.AddAuthentication(options =>
       {
           options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       })
       .AddJwtBearer(options =>
       {
           options.Authority = "https://eventregistrar.eu.auth0.com/";
           options.Audience = "https://eventregistrar.azurewebsites.net/api";
       });
builder.Services.AddAuthorization(o => o.AddPolicy("api", p => p.RequireAuthenticatedUser()));
builder.Services.AddCors();

//builder.Services.AddSwaggerDocument();
builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider>(_ => new MediatorEndpointApiDescriptionGroupCollectionProvider(container.GetInstance<RequestRegistry>()));
builder.Services.AddOpenApiDocument(document => document.DocumentName = "v1");
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    options.DeveloperMode = builder.Environment.IsDevelopment();
});
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, _) => { module.EnableSqlCommandTextInstrumentation = true; });

var app = builder.Build();

((IApplicationBuilder)app).UseSimpleInjector(container);
var assemblies = new[] { typeof(Program).Assembly };

container.RegisterMediatr(assemblies);
container.RegisterDataAccessToSqlServer(assemblies,
                                        builder.Configuration.GetConnectionString("DefaultConnection"));
container.RegisterIamWithAuth0();
container.RegisterEventBus(assemblies);
container.RegisterAzureServiceBus(builder.Configuration.GetValue<string>("ServiceBus_ConnectionString"),
                                  builder.Configuration.GetValue<string>("ServiceBusNamespace"));
container.RegisterErrorHandling(assemblies);
container.RegisterConfiguration(assemblies);
container.RegisterMisc(assemblies);

container.Verify();

container.GetInstance<MessageQueueReceiver>().StartReceiveLoop();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}


app.UseOpenApi();
app.UseSwaggerUi();

app.UseRequestLocalization();
app.UseMiddleware<ExceptionMiddleware>(container);


app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());


app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRequests(container);
    endpoints.MapHub<NotificationHub>("/notifications");
});

app.MapGet("/", () => container.GetInstance<HomeController>().Index()).AllowAnonymous();

app.MapPost("api/events/{eventAcronym}/paymentfiles/upload", UploadPaymentFile)
   .DisableAntiforgery();

app.Run();


async Task UploadPaymentFile(string eventAcronym, IFormFile file)
{
    var stream = new MemoryStream((int)file.Length);
    await file.CopyToAsync(stream);
    await container.GetInstance<IMediator>()
                   .Send(new SavePaymentFileCommand
                         {
                             EventId = await container.GetInstance<IEventAcronymResolver>().GetEventIdFromAcronym(eventAcronym),
                             FileStream = stream,
                             Filename = file.FileName,
                             ContentType = file.ContentType
                         });
}


public partial class Program;