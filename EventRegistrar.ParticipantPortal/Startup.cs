using System.Linq;

using EventRegistrar.Backend.Infrastructure.DataAccess;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;

namespace EventRegistrar.ParticipantPortal
{
    public class Startup
    {
        private readonly Container _container = new Container();

        public Startup(IConfiguration configuration)
        {
            _container.Options.ResolveUnregisteredConcreteTypes = false;

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = GetType().Assembly;
            services.AddMvc()
                    ;//.AddApplicationPart(assembly);

            SetupContainer(_container);
            services.AddSimpleInjector(_container, options =>
            {
                // AddAspNetCore() wraps web requests in a Simple Injector scope and
                // allows request-scoped framework services to be resolved.
                options.AddAspNetCore()

                       // Ensure activation of a specific framework type to be created by
                       // Simple Injector instead of the built-in configuration system.
                       // All calls are optional. You can enable what you need. For instance,
                       // PageModels and TagHelpers are not needed when you build a Web API.
                       .AddControllerActivation();

                // Optionally, allow application components to depend on the non-generic
                // ILogger (Microsoft.Extensions.Logging) or IStringLocalizer
                // (Microsoft.Extensions.Localization) abstractions.
                options.AddLogging();
                //options.AddLocalization();
                //options.CrossWire<>()
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(_container));
            services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(_container));

            //services.EnableSimpleInjectorCrossWiring(_container);
            services.UseSimpleInjectorAspNetRequestScoping(_container);


            services.AddRazorPages();
            //services.UseSimpleInjector(_container);
            services.AddSingleton(_container);
            //services.AddApplicationInsightsTelemetry();

        }

        private void SetupContainer(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;

            container.RegisterInstance(GetDbOptions());
            var assemblies = new[] { GetType().Assembly };
            container.Register(typeof(IRequestHandler<>), assemblies);
            container.Register(typeof(IRequestHandler<,>), assemblies);
            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);
            container.Register(typeof(IQueryable<>), typeof(Queryable<>));
            container.Register(typeof(IRepository<>), typeof(Repository<>));

            //var dbOptions = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            //dbOptions.UseInMemoryDatabase("InMemoryDb");
            //container.RegisterInstance(dbOptions.Options);
            container.Register<DbContext, EventRegistratorDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleInjector(_container);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            _container.Verify();
        }

        protected virtual DbContextOptions<EventRegistratorDbContext> GetDbOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString, builder =>
            {
                builder.EnableRetryOnFailure();
            });
            optionsBuilder.EnableSensitiveDataLogging();

            return optionsBuilder.Options;
        }
    }
}
