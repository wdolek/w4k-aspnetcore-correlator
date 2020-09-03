using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;
using Xunit;

namespace W4k.AspNetCore.Correlator.Options
{
    public class CorrelatorOptionsTests
    {
        [Fact]
        public void Invoke_WhenMisconfigured_ExpectOptionsValidationException()
        {
            Assert.Throws<OptionsValidationException>(() =>
            {
                using var _ = new TestServer(CreateTestWebHostBuilder());
            });
        }

        private static IWebHostBuilder CreateTestWebHostBuilder() =>
            new WebHostBuilder()
                .UseEnvironment("test")
                .UseStartup<LocalStartup>();

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
                app.Use(async (context, next) =>
                {
                    await next();
                });
            }
        }
    }
}
