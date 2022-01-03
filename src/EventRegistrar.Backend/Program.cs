using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using SimpleInjector;
using SimpleInjector.Lifestyles;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var container = new Container();
container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
container.Options.ResolveUnregisteredConcreteTypes = true;
container.Options.DefaultLifestyle = Lifestyle.Scoped;

// Add services to the container.
builder.Services.AddMvc() //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
    //.AddNewtonsoftJson(options =>
    //{
    //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    //    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
    //    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
    //});
    ;
builder.Services.AddMemoryCache();
builder.Services.AddSimpleInjector(container, options =>
{
    // AddAspNetCore() wraps web requests in a Simple Injector scope and
    // allows request-scoped framework services to be resolved.
    options.AddAspNetCore()

           // Ensure activation of a specific framework type to be created by
           // Simple Injector instead of the built-in configuration system.
           // All calls are optional. You can enable what you need. For instance,
           // ViewComponents, PageModels, and TagHelpers are not needed when you
           // build a Web API.
           .AddControllerActivation();
    //.AddViewComponentActivation()
    //.AddPageModelActivation()
    //.AddTagHelperActivation();

    // Optionally, allow application components to depend on the non-generic
    // ILogger (Microsoft.Extensions.Logging) or IStringLocalizer
    // (Microsoft.Extensions.Localization) abstractions.
    options.AddLogging();
    //options.AddLocalization();
});
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

var app = builder.Build();

((IApplicationBuilder)app).UseSimpleInjector(container);
//_container.CrossWire<IMemoryCache>(app);
//_container.CrossWire<ILoggerFactory>(app);

var assemblies = new[] { typeof(Program).Assembly };

container.Register(typeof(IRequestHandler<>), assemblies);
container.Register(typeof(IRequestHandler<,>), assemblies);
container.Collection.Register(typeof(IEventToCommandTranslation<>), assemblies);

container.RegisterSingleton<IMediator, Mediator>();
container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);

container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
                                                            {
                                                                typeof(ExtractEventIdDecorator<,>),
                                                                typeof(AuthorizationDecorator<,>),
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
container.Register(() => new AuthenticatedUserId(container.GetInstance<IAuthenticatedUserProvider>()
                                                          .GetAuthenticatedUserId()
                                                          .Result));
container.Register(() => container.GetInstance<IAuthenticatedUserProvider>().GetAuthenticatedUser());

container.Register<IEventAcronymResolver, EventAcronymResolver>();
container.Register<IAuthorizationChecker, AuthorizationChecker>();
container.Register<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
container.Register<IRightsOfEventRoleProvider, RightsOfEventRoleProvider>();

//container.Register(() => container.GetInstance<IHttpContextAccessor>().HttpContext?.Request ?? new DefaultHttpRequest(new DefaultHttpContext()));

// Configuration
container.Register<ConfigurationResolver>();
var defaultConfigItemTypes = container.GetTypesToRegister<IDefaultConfigurationItem>(assemblies).ToList();
container.Collection.Register<IDefaultConfigurationItem>(defaultConfigItemTypes);

var domainEventTypes = container.GetTypesToRegister<DomainEvent>(assemblies);
container.RegisterSingleton(() => new DomainEventCatalog(domainEventTypes));

var configTypes = container.GetTypesToRegister<IConfigurationItem>(assemblies)
                           .Except(defaultConfigItemTypes);
foreach (var configType in configTypes)
    container.Register(configType,
        () => container.GetInstance<ConfigurationResolver>().GetConfigurationTypeless(configType));

var serviceBusConsumers = container.GetTypesToRegister<IQueueBoundMessage>(assemblies)
                                   .Select(type => new ServiceBusConsumer
                                                   {
                                                       QueueName =
                                                           ((IQueueBoundMessage)Activator.CreateInstance(type)!)
                                                           .QueueName,
                                                       RequestType = type
                                                   })
                                   .ToList();
container.RegisterInstance<IEnumerable<ServiceBusConsumer>>(serviceBusConsumers);
container.RegisterSingleton<MessageQueueReceiver>();
container.Register<ServiceBusClient>();
container.Register<IEventBus, EventBus>();
container.Register<SourceQueueProvider>();
container.Register(typeof(IEventToUserTranslation<>), assemblies);
container.Verify();

container.GetInstance<MessageQueueReceiver>().RegisterMessageHandlers();


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


app.UseRequestLocalization();
app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        "default",
        "{controller}/{action=Index}/{id?}");
});


app.Run();


void SetDbOptions(DbContextOptionsBuilder o)
{
    o.UseSqlServer(connectionString, sqlBuilder => { sqlBuilder.EnableRetryOnFailure(); })
     .EnableSensitiveDataLogging();
}