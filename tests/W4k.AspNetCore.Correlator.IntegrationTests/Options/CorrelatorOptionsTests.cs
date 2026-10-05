using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace W4k.AspNetCore.Correlator.Options;

public class CorrelatorOptionsTests
{
    [Test]
    public void Invoke_WhenMisconfigured_ExpectOptionsValidationException()
    {
        Assert.Throws<OptionsValidationException>(() =>
        {
            using var host = CreateTestWebHostBuilder<LocalStartup>().Build();
            host.Start();

            _ = host.GetTestServer();
        });
    }

    [Test]
    public async Task Invoke_WhenInvalidHeaderNameConfigured_ExpectOptionsValidationException()
    {
        var exception = Assert.Throws<OptionsValidationException>(() =>
        {
            using var host = CreateTestWebHostBuilder<InvalidHeaderNameStartup>().Build();
            host.Start();

            _ = host.GetTestServer();
        });

        await Assert.That(exception.Message).Contains("X CID");
    }

    private static IHostBuilder CreateTestWebHostBuilder<TStartup>()
        where TStartup : class =>
        new HostBuilder().ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder
                .UseEnvironment("test")
                .UseTestServer()
                .UseStartup<TStartup>();
        });

    private class LocalStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(o =>
            {
                o.ReadFrom.Clear();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Use(async (_, next) =>
            {
                await next();
            });
        }
    }

    private class InvalidHeaderNameStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(o =>
            {
                o.ReadFrom.Clear();
                o.ReadFrom.Add("X CID");
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Use(async (_, next) =>
            {
                await next();
            });
        }
    }
}
