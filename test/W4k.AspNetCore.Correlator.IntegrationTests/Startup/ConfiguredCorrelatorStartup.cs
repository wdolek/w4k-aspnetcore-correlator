using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Startup
{
    public class ConfiguredCorrelatorStartup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services.AddDefaultCorrelator(
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

                    // overwrite `HttpContext.TraceIdentifier` by correlation ID
                    o.ReplaceTraceIdentifier = true;

                    // enable logging scope containing correlation ID
                    o.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("Correlation");
                });

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Run(async (context) =>
            {
                var correlationId = context.TraceIdentifier;

                context.Response.Headers.Append("Content-Type", "text/plain");
                await context.Response.WriteAsync(correlationId, context.RequestAborted);
                
                await Task.Delay(127);
            });
        }
    }
}
