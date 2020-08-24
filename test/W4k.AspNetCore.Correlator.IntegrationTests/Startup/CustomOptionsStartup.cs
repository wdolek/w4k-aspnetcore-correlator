using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.IntegrationTests.Startup
{
    public class CustomOptionsStartup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services.AddCorrelator(
                o =>
                {
                    // disable correlation ID factory
                    // (if correlation ID not sent, ASP.NET trace ID is going to be used)
                    o.Factory = null;

                    // read custom header
                    o.ReadFrom.Clear();
                    o.ReadFrom.Add("X-CID");

                    // do not emit correlation ID
                    o.Emit = PropagationSettings.KeepIncomingHeaderName();

                    // don't overwrite `HttpContext.TraceIdentifier` by correlation ID
                    o.ReplaceTraceIdentifier = false;

                    // enable logging scope containing correlation ID
                    o.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("correlation");
                });

        public void Configure(IApplicationBuilder app, ICorrelationContextAccessor correlationContextAccessor)
        {
            app.UseCorrelator();
            app.Use(async (context, next) =>
            {
                var correlationId = correlationContextAccessor.CorrelationContext.CorrelationId;

                context.Response.Headers.Add("X-Test-Correlation-ID", (string)correlationId);
                context.Response.Headers.Add("Content-Type", "text/plain");

                await context.Response.WriteAsync(correlationId);
            });
        }
    }
}
