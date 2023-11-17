using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Benchmarks.Startup;

internal class ValidatingCorrelatorStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // all correlation ID values used will be valid
        services.AddDefaultCorrelator()
            .WithValidator(new CorrelationValueLengthValidator(40));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCorrelator();
        app.UseMiddleware<CorrelatedMiddleware>();
        app.UseMiddleware<DummyMiddleware>();
    }
}
