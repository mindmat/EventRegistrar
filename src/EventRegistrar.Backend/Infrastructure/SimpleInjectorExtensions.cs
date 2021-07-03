namespace EventRegistrar.Backend.Infrastructure
{
    public static class SimpleInjectorExtensions
    {
        //public static void UseSimpleInjector(this IServiceCollection services, Container container)
        //{

        //    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        //    services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));
        //    services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(container));

        //    services.EnableSimpleInjectorCrossWiring(container);
        //    services.UseSimpleInjectorAspNetRequestScoping(container);
        //}

        //public static void UseSimpleInjectorInternal(this IApplicationBuilder app, Container container)
        //{
        //    //container.RegisterMvcControllers(app);
        //    //container.RegisterMvcViewComponents(app);

        //    //// Allow Simple Injector to resolve services from ASP.NET Core.
        //    //container.AutoCrossWireAspNetComponents(app);
        //}
    }
}