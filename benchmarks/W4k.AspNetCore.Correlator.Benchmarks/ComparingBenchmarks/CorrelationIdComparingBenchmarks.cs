using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Helpers;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Benchmarks.ComparingBenchmarks;

[BenchmarkCategory("Comparison", "CorrelationId")]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[MemoryDiagnoser]
public class CorrelationIdComparingBenchmarks : IDisposable
{
    private readonly TestServerContainer _correlator;
    private readonly TestServerContainer _correlationId;

    public CorrelationIdComparingBenchmarks()
    {
        _correlator =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelatorStartup>()));

        _correlationId =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelationIdStartup>()));
    }

    [Benchmark(Description = "Correlator: Short", Baseline = true)]
    [BenchmarkCategory("Short")]
    public async Task CorrelatorRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "CorrelationId: Short")]
    [BenchmarkCategory("Short")]
    public async Task CorrelationIdRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlationId.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "Correlator: Long", Baseline = true)]
    [BenchmarkCategory("Long")]
    public async Task CorrelatorRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "CorrelationId: Long")]
    [BenchmarkCategory("Long")]
    public async Task CorrelationIdRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlationId.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "Correlator: Empty", Baseline = true)]
    [BenchmarkCategory("None")]
    public async Task CorrelatorRequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark(Description = "CorrelationId: Empty")]
    [BenchmarkCategory("None")]
    public async Task CorrelationIdRequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _correlationId.HttpClient.SendAsync(requestMessage);
    }

    public void Dispose()
    {
        _correlator?.Dispose();
        _correlationId?.Dispose();
    }

    private class CorrelatorStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(options =>
            {
                options.ReadFrom.Clear();
                options.ReadFrom.Add("X-Correlation-Id");

                options.Factory = (_) => CorrelationId.FromString("le_correlation");
                options.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
                options.ReplaceTraceIdentifier = false;
                options.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("Correlation");
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.UseMiddleware<DummyMiddleware>();
        }
    }

    private class CorrelationIdStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelationId(options =>
            {
                options.CorrelationIdGenerator = () => "le_correlation";
                options.AddToLoggingScope = true;
                options.LoggingScopeKey = "Correlation";
                options.EnforceHeader = false;
                options.IgnoreRequestHeader = false;
                options.IncludeInResponse = true;
                options.RequestHeader = "X-Correlation-Id";
                options.ResponseHeader = "X-Correlation-Id";
                options.UpdateTraceIdentifier = false;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelationId();
            app.UseMiddleware<DummyMiddleware>();
        }
    }
}
