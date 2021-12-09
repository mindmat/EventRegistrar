using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace EventRegistrar.Backend.Infrastructure;

public class EnsureExecutionScope : IDisposable
{
    private readonly Scope _scope;

    public EnsureExecutionScope(Container container, bool force = false)
    {
        if (force || Lifestyle.Scoped.GetCurrentScope(container) == null)
            _scope = AsyncScopedLifestyle.BeginScope(container);
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}