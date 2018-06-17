using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EventRegistrator.Web
{
    public class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>();

        //.UseSerilog(ConfigureSerilog);

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}