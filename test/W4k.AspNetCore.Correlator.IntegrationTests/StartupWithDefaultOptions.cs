using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class StartupWithDefaultOptions
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // no trace of CorrelatorOptions configuration!
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelatorMiddleware>();
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("X-Test-Correlation-ID", context.TraceIdentifier);
                return next.Invoke();
            });
        }
    }
}
