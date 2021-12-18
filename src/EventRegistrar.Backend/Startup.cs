using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace EventRegistrar.Backend;

public class Startup
{
    private readonly Container _container = new();

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        _container.Options.ResolveUnregisteredConcreteTypes = true;
        _container.Options.DefaultLifestyle = Lifestyle.Scoped;

        services.AddMvc() //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            //.AddNewtonsoftJson(options =>
            //{
            //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
            //    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            //});
            ;
        services.AddMemoryCache();
        services.AddSimpleInjector(_container, options =>
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
        services.AddSingleton(_container);
        //services.AddApplicationInsightsTelemetry();

        //services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSimpleInjector(_container);
        _container.RegisterInstance(GetDbOptions());
        //_container.CrossWire<IMemoryCache>(app);
        //_container.CrossWire<ILoggerFactory>(app);
        SetIdentityProvider(_container);
        //CompositionRoot.RegisterTypes(_container);
        OverrideRegistrations();
        _container.Verify();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRequestLocalization();

        app.UseCors(builder => builder.AllowAnyOrigin()
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
    }

    protected virtual void OverrideRegistrations()
    {
        return;
    }

    protected virtual DbContextOptions<EventRegistratorDbContext> GetDbOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString, builder => { builder.EnableRetryOnFailure(); });
        optionsBuilder.EnableSensitiveDataLogging();

        return optionsBuilder.Options;
    }

    protected virtual void SetIdentityProvider(Container container)
    {
        container.Register<IIdentityProvider, GoogleIdentityProvider>();
    }
}