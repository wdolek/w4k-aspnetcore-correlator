using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Benchmarks.Startup;

internal class ConfiguredCorrelatorStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultCorrelator(options =>
        {
            options.Emit = PropagationSettings.KeepIncomingHeaderName();
            options.ReplaceTraceIdentifier = true;
            options.LoggingScope = LoggingScopeSettings.IncludeLoggingScope();

            options.ReadFrom.Add("X-Correlation");
            options.ReadFrom.Add("X-Request");
            options.ReadFrom.Add("X-Trace-Id");
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCorrelator();
        app.UseMiddleware<CorrelatedMiddleware>();
        app.UseMiddleware<DummyMiddleware>();
    }
}