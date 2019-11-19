using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EventRegistrator.Web
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>();
                       });
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        //private static void ConfigureSerilog(WebHostBuilderContext context, LoggerConfiguration configuration)
        //{
        //    configuration
        //        .MinimumLevel.Debug()
        //        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        //        .Enrich.FromLogContext()
        //        .WriteTo.ApplicationInsightsTraces(context.Configuration["ApplicationInsights:InstrumentationKey"]);
        //}
    }
}