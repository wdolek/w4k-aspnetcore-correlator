using System;
using System.Threading.Tasks;
using AspNet.CorrelationIdGenerator.ApplicationBuilderExtensions;
using AspNet.CorrelationIdGenerator.ServiceCollectionExtensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Benchmarks.ComparingBenchmarks;

[BenchmarkCategory("Comparison", "AspNet.CorrelationIdGenerator")]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[MemoryDiagnoser]
public class CorrelationIdGeneratorComparingBenchmarks : IDisposable
{
    private readonly TestServerContainer _correlator;
    private readonly TestServerContainer _correlationIdGenerator;

    public CorrelationIdGeneratorComparingBenchmarks()
    {
        _correlator =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelatorStartup>()));

        _correlationIdGenerator =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelationIdGeneratorStartup>()));
    }


    [Benchmark(Description = "Correlator: Short", Baseline = true)]
    [BenchmarkCategory("Short")]
    public async Task CorrelatorRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "AspNet.CorrelationIdGenerator: Short")]
    [BenchmarkCategory("Short")]
    public async Task CorrelateRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlationIdGenerator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "Correlator: Long", Baseline = true)]
    [BenchmarkCategory("Long")]
    public async Task CorrelatorRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "AspNet.CorrelationIdGenerator: Long")]
    [BenchmarkCategory("Long")]
    public async Task CorrelateRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlationIdGenerator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "Correlator: Empty", Baseline = true)]
    [BenchmarkCategory("None")]
    public async Task CorrelatorRequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "AspNet.CorrelationIdGenerator: Empty")]
    [BenchmarkCategory("None")]
    public async Task CorrelateRequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _correlationIdGenerator.HttpClient.SendAsync(requestMessage);
    }

    public void Dispose()
    {
        _correlator?.Dispose();
        _correlationIdGenerator?.Dispose();
    }

    private class CorrelatorStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(options =>
            {
                options.Factory = _ =>
                    CorrelationId.FromString(Guid.NewGuid().ToString());

                options.Emit = PropagationSettings.NoPropagation;
                options.ReplaceTraceIdentifier = false;
                options.LoggingScope = LoggingScopeSettings.NoScope;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.UseMiddleware<DummyMiddleware>();
        }
    }

    private class CorrelationIdGeneratorStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelationIdGenerator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.AddCorrelationIdMiddleware();
            app.UseMiddleware<DummyMiddleware>();
        }
    }
}