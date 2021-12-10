using EventRegistrar.Backend;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using SimpleInjector;
using SimpleInjector.Lifestyles;

var builder = WebApplication.CreateBuilder(args);
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
container.RegisterInstance(GetDbOptions());
//_container.CrossWire<IMemoryCache>(app);
//_container.CrossWire<ILoggerFactory>(app);
container.Register<IIdentityProvider, Auth0IdentityProvider>();

CompositionRoot.RegisterTypes(container);
container.Verify();


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


DbContextOptions<EventRegistratorDbContext> GetDbOptions()
{
    var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
    optionsBuilder.UseSqlServer(connectionString, sqlBuilder => { sqlBuilder.EnableRetryOnFailure(); });
    optionsBuilder.EnableSensitiveDataLogging();

    return optionsBuilder.Options;
}