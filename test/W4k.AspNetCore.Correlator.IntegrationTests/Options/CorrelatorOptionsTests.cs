using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.Options;

public class CorrelatorOptionsTests
{
    [Fact]
    public void Invoke_WhenMisconfigured_ExpectOptionsValidationException()
    {
        Assert.Throws<OptionsValidationException>(() =>
        {
            using var host = CreateTestWebHostBuilder().Build();
            host.Start();

            _ = host.GetTestServer();
        });
    }

    private static IHostBuilder CreateTestWebHostBuilder() =>
        new HostBuilder().ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder
                .UseEnvironment("test")
                .UseTestServer()
                .UseStartup<LocalStartup>();
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
}