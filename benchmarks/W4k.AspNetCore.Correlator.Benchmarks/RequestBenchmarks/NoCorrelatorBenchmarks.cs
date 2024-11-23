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

namespace W4k.AspNetCore.Correlator.Benchmarks.RequestBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class NoCorrelatorBenchmarks : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public NoCorrelatorBenchmarks()
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<NoCorrelatorStartup>());
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

file class NoCorrelatorStartup
{
    public void ConfigureServices(IServiceCollection _)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<DummyMiddleware>();
    }
}