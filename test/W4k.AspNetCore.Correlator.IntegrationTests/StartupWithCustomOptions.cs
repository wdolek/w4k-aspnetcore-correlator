using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelatorMiddleware>();
            app.Use((context, next) => {
                context.Response.Headers.Add("X-Test-CorrelationId", context.TraceIdentifier);
                return next.Invoke();
            });
        }
    }
}
