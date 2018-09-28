using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Extensions;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class StartupWithDefaultOptions
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("X-Test-Correlation-ID", context.TraceIdentifier);
                return next.Invoke();
            });
        }
    }
}
