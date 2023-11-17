using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.Startup
{
    public class DefaultCorrelatorStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator();
        }

        public void Configure(IApplicationBuilder app, ICorrelationContextAccessor correlationContextAccessor)
        {
            app.UseCorrelator();
            app.Run(async (context) =>
            {
                var correlationId = correlationContextAccessor.CorrelationContext.CorrelationId;

                context.Response.Headers.Append("Content-Type", "text/plain");
                await context.Response.WriteAsync(correlationId, context.RequestAborted);

                await Task.Delay(127);
            });
        }
    }
}
