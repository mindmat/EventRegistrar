using Microsoft.Extensions.Hosting;

namespace EventRegistrar.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
                   .ConfigureFunctionsWorkerDefaults()
                   .Build();

        host.Run();
    }
}