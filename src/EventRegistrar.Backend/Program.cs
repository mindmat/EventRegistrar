using Azure.Identity;
using Azure.Messaging.ServiceBus;

using EventRegistrar.Backend;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

using SimpleInjector;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var container = new Container();
container.Options.ResolveUnregisteredConcreteTypes = true;
container.Options.DefaultLifestyle = Lifestyle.Scoped;
//container.Options.EnableAutoVerification = false;

// Add services to the container.
//builder.Services.AddMvc() //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
//.AddNewtonsoftJson(options =>
//{
//    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
//    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
//    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
//});

builder.Services.AddMemoryCache();
builder.Services.AddSimpleInjector(container, options =>
{
    // AddAspNetCore() wraps web requests in a Simple Injector scope and
    // allows request-scoped framework services to be resolved.
    options.AddAspNetCore();

    //       // Ensure activation of a specific framework type to be created by
    //       // Simple Injector instead of the built-in configuration system.
    //       // All calls are optional. You can enable what you need. For instance,
    //       // ViewComponents, PageModels, and TagHelpers are not needed when you
    //       // build a Web API.
    //.AddControllerActivation();
    //.AddViewComponentActivation()
    //.AddPageModelActivation()
    //.AddTagHelperActivation();

    // Optionally, allow application components to depend on the non-generic
    // ILogger (Microsoft.Extensions.Logging) or IStringLocalizer
    // (Microsoft.Extensions.Localization) abstractions.
    options.AddLogging();
    //options.AddLocalization();
});
builder.Services.AddSignalR();
builder.Services.AddSingleton(container);
//builder.Services.AddDbContext<EventRegistratorDbContext>(SetDbOptions);

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

var app = builder.Build();

((IApplicationBuilder)app).UseSimpleInjector(container);
//_container.CrossWire<IMemoryCache>(app);
//_container.CrossWire<ILoggerFactory>(app);

var assemblies = new[] { typeof(Program).Assembly };

var requestTypes = container.GetTypesToRegister(typeof(IRequestHandler<,>), assemblies);
container.RegisterSingleton(() => new RequestRegistry(requestTypes));

container.Register(typeof(IRequestHandler<>), assemblies);
container.Register(typeof(IRequestHandler<,>), assemblies);
container.Collection.Register(typeof(IEventToCommandTranslation<>), assemblies);

container.RegisterSingleton<IMediator, Mediator>();
container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);
container.Register<IHttpContextAccessor, HttpContextContainer>();
container.Register<EventContext>();

container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
                                                            {
                                                                typeof(ExtractEventIdDecorator<,>),
                                                                //typeof(AuthorizationDecorator<,>),
                                                                typeof(CommitUnitOfWorkDecorator<,>)
                                                            });

container.Register(typeof(IQueryable<>), typeof(Queryable<>));
container.Register(typeof(IRepository<>), typeof(Repository<>));

var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
SetDbOptions(optionsBuilder);
container.RegisterInstance(optionsBuilder);
container.RegisterInstance(optionsBuilder.Options);
container.Register<DbContext, EventRegistratorDbContext>();

container.Register<IIdentityProvider, Auth0IdentityProvider>();
container.RegisterSingleton<Auth0TokenProvider>();
container.Register(() => new AuthenticatedUserId(container.GetInstance<IAuthenticatedUserProvider>()
                                                          .GetAuthenticatedUserId()
                                                          .Result));
container.Register(() => container.GetInstance<IAuthenticatedUserProvider>()
                                  .GetAuthenticatedUser());
container.Collection.Register(typeof(IReadModelUpdater), assemblies);

container.Register<IEventAcronymResolver, EventAcronymResolver>();
container.Register<IAuthorizationChecker, AuthorizationChecker>();
container.Register<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
container.Register<IRightsOfEventRoleProvider, RightsOfEventRoleProvider>();

container.RegisterSingleton<IDateTimeProvider, DateTimeProvider>();

container.RegisterSingleton(() => new ServiceBusClient(builder.Configuration.GetValue<string>("ServiceBusNamespace"), new DefaultAzureCredential()));
container.RegisterSingleton(() => container.GetInstance<ServiceBusClient>().CreateSender(CommandQueue.CommandQueueName));

// Error handling
container.RegisterSingleton<ExceptionTranslator>();

//var exceptionTranslation = assemblies.GetTypes<IExceptionTranslation>().ToArray();
//exceptionTranslation.DoForEach(ext => container.RegisterSingleton(ext, ext));
container.Collection.Register<IExceptionTranslation>(assemblies);

//container.Register(() => container.GetInstance<IHttpContextAccessor>().HttpContext?.Request ?? new DefaultHttpRequest(new DefaultHttpContext()));

// Configuration
container.Register<ConfigurationRegistry>();
var defaultConfigItemTypes = container.GetTypesToRegister<IDefaultConfigurationItem>(assemblies)
                                      .ToList();
container.Collection.Register<IDefaultConfigurationItem>(defaultConfigItemTypes, Lifestyle.Singleton);

var domainEventTypes = container.GetTypesToRegister<DomainEvent>(assemblies);
container.RegisterSingleton(() => new DomainEventCatalog(domainEventTypes));

var configTypes = container.GetTypesToRegister<IConfigurationItem>(assemblies)
                           .Except(defaultConfigItemTypes);
foreach (var configType in configTypes)
{
    container.Register(configType, () => container.GetInstance<ConfigurationRegistry>().GetConfigurationTypeless(configType));
    // add the possibility to inject FeatureConfigurations to singletons
    container.RegisterSingleton(typeof(SingletonConfigurationFeature<>).MakeGenericType(configType),
                                () => GetSingletonConfig(container, configType));
}


container.RegisterSingleton<SecretReader>();
container.RegisterSingleton<MessageQueueReceiver>();
container.Register<CommandQueue>();
container.Register<IEventBus, EventBus>();
container.Register<SourceQueueProvider>();
container.Register(typeof(IEventToUserTranslation<>), assemblies);
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
app.UseSwaggerUi3();

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
    endpoints.MapGet("/", () => container.GetInstance<HomeController>().Index()).AllowAnonymous();
});


app.Run();


void SetDbOptions(DbContextOptionsBuilder o)
{
    o.UseSqlServer(connectionString, sqlBuilder => { sqlBuilder.EnableRetryOnFailure(); })
     .EnableSensitiveDataLogging();
}


static object GetSingletonConfig(Container container, Type featureConfigType)
{
    using (new EnsureExecutionScope(container))
    {
        var singletonConfigurationFeatureType = typeof(SingletonConfigurationFeature<>).MakeGenericType(featureConfigType);
        var constructor = singletonConfigurationFeatureType.GetConstructor(new[] { featureConfigType });
        return constructor?.Invoke(new[] { container.GetInstance(featureConfigType) })!;
    }
}