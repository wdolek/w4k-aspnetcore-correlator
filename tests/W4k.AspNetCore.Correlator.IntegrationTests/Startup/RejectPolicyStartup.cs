using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Startup;

public class RejectPolicyStartup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddDefaultCorrelator(
                o =>
                {
                    o.ReadFrom.Clear();
                    o.ReadFrom.Add("X-CID");
                    o.Emit = PropagationSettings.KeepIncomingHeaderName();

                    // reject request when received value is invalid
                    o.InvalidValuePolicy = InvalidCorrelationPolicy.Reject;
                })
            .WithValidator(new CorrelationValueLengthValidator(8));

    public void Configure(IApplicationBuilder app)
    {
        app.UseCorrelator();
        app.Run(async context =>
        {
            context.Response.Headers.Append("Content-Type", "text/plain");
            await context.Response.WriteAsync("should not be reached");
        });
    }
}
