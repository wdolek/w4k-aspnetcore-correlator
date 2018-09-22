using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class StartupWithDefaultOptions
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CorrelatorOptions>(o => { });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelatorMiddleware>();
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("X-Test-CorrelationId", context.TraceIdentifier);
                return next.Invoke();
            });
        }
    }
}
