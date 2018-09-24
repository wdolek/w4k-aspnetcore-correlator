using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class StartupWithCustomOptions
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CorrelatorOptions>(
                o => {
                    // disable correlation ID factory
                    // (if correlation ID not sent, ASP.NET trace ID is going to be used)
                    o.Factory = null;

                    // read custom header
                    o.ReadFrom.Clear();
                    o.ReadFrom.Add("X-CID");

                    // do not emit correlation ID
                    o.Emit = PropagationSettings.NoPropagation;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelatorMiddleware>();
            app.Use((context, next) => {
                context.Response.Headers.Add("X-Test-Correlation-ID", context.TraceIdentifier);
                return next.Invoke();
            });
        }
    }
}
