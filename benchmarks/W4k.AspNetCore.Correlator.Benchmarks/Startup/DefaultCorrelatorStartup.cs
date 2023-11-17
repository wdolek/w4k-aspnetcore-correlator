using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;

namespace W4k.AspNetCore.Correlator.Benchmarks.Startup;

internal class DefaultCorrelatorStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultCorrelator();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCorrelator();
        app.UseMiddleware<CorrelatedMiddleware>();
        app.UseMiddleware<DummyMiddleware>();
    }
}
