using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Benchmarks.RequestBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ConfiguredCorrelatorBenchmarks : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public ConfiguredCorrelatorBenchmarks()
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<ConfiguredCorrelatorStartup>());
        _client = _server.CreateClient();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Short")]
    public async Task CorrelatedRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _client.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("Long")]
    public async Task CorrelatedRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _client.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("None")]
    public async Task RequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _client.SendAsync(requestMessage);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }
}

file class ConfiguredCorrelatorStartup
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