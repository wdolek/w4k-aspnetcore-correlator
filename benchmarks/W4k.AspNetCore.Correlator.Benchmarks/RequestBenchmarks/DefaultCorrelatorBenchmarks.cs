using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using W4k.AspNetCore.Correlator.Benchmarks.Helpers;
using W4k.AspNetCore.Correlator.Benchmarks.Startup;

namespace W4k.AspNetCore.Correlator.Benchmarks.RequestBenchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public sealed class DefaultCorrelatorBenchmarks : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public DefaultCorrelatorBenchmarks()
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<DefaultCorrelatorStartup>());
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
