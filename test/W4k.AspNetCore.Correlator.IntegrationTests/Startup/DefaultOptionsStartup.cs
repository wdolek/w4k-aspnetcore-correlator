using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.IntegrationTests.Startup
{
    public class DefaultOptionsStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Use(async (context, next) =>
            {
                // by default, correlation ID is written to `HttpContext.TraceIdentifier`
                var correlationId = context.TraceIdentifier;

                context.Response.Headers.Add("X-Test-Correlation-ID", correlationId);
                context.Response.Headers.Add("Content-Type", "text/plain");

                await context.Response.WriteAsync(correlationId);
            });
        }
    }
}
