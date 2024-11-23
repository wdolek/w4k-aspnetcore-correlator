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
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Benchmarks.RequestBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ValidatingCorrelatorBenchmarks : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public ValidatingCorrelatorBenchmarks()
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<ValidatingCorrelatorStartup>());
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

file class ValidatingCorrelatorStartup
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