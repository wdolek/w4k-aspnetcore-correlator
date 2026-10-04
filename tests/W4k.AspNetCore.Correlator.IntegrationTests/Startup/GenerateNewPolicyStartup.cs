using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Options;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Startup;

public class GenerateNewPolicyStartup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddDefaultCorrelator(
                o =>
                {
                    o.ReadFrom.Clear();
                    o.ReadFrom.Add("X-CID");

                    // emit correlation ID with incoming header name
                    o.Emit = PropagationSettings.KeepIncomingHeaderName();

                    // generate new correlation ID when received value is invalid
                    o.InvalidValuePolicy = InvalidCorrelationPolicy.GenerateNew;
                })
            .WithValidator(new CorrelationValueLengthValidator(8));

    public void Configure(IApplicationBuilder app)
    {
        app.UseCorrelator();
        app.Run(async context =>
        {
            var contextAccessor = context.RequestServices.GetRequiredService<ICorrelationContextAccessor>();

            context.Response.Headers.Append("Content-Type", "text/plain");
            await context.Response.WriteAsync(contextAccessor.CorrelationContext.CorrelationId);
        });
    }
}
