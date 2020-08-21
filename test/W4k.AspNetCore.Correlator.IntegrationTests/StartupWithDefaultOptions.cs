using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class StartupWithDefaultOptions
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelator();
        }

        public void Configure(IApplicationBuilder app, ICorrelationContextAccessor correlationContextAccessor)
        {
            app.UseCorrelator();
            app.Use((context, next) =>
            {
                context.Response.Headers.Add(
                    "X-Test-Correlation-ID",
                    new StringValues(correlationContextAccessor.CorrelationContext.CorrelationId));

                return next.Invoke();
            });
        }
    }
}
