using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class SimpleInjectorServiceProvider(Container container) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return container.GetInstance(serviceType);
    }
}