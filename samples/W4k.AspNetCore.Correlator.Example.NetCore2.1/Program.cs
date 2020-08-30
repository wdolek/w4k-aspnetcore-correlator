using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace W4k.AspNetCore.Correlator.Example.NetCore21
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole(options =>
                    {
                        options.DisableColors = true;
                        options.IncludeScopes = true;
                    });
                })
                .UseStartup<Startup>();
    }
}
