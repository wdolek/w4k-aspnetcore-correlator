using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;

namespace W4k.AspNetCore.Correlator.Benchmarks.Startup;

internal class NoCorrelatorStartup
{
    public void ConfigureServices(IServiceCollection _)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<DummyMiddleware>();
    }
}
