using System;
using System.Globalization;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Benchmarks.Helpers;
using W4k.AspNetCore.Correlator.Benchmarks.Middleware;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Benchmarks.ComparingBenchmarks;

[BenchmarkCategory("Comparison", "Correlate")]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CorrelateComparingBenchmarks : IDisposable
{
    private readonly TestServerContainer _correlator;
    private readonly TestServerContainer _correlationId;

    public CorrelateComparingBenchmarks()
    {
        _correlator =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelatorStartup>()));

        _correlationId =
            new TestServerContainer(
                new TestServer(
                    new WebHostBuilder().UseStartup<CorrelateStartup>()));
    }

    [Benchmark]
    [BenchmarkCategory("Short")]
    public async Task CorrelatorRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("Short")]
    public async Task CorrelateRequest()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequest();
        await _correlationId.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("Long")]
    public async Task CorrelatorRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("Long")]
    public async Task CorrelateRequestWithAdditionalHeaders()
    {
        var requestMessage = RequestFactory.CreateCorrelatedRequestWithAdditionalHeaders();
        await _correlationId.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("None")]
    public async Task CorrelatorRequestWithoutCorrelation()
    {
        var requestMessage = RequestFactory.CreateRequestWithoutCorrelation();
        await _correlator.HttpClient.SendAsync(requestMessage);
    }

    [Benchmark]
    [BenchmarkCategory("None")]
    public async Task CorrelateRequestWithoutCorrelation()
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

                options.Factory = (_) =>
                    CorrelationId.FromString(
                        Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture));

                options.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
                options.ReplaceTraceIdentifier = false;
                options.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("CorrelationId");
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.UseMiddleware<DummyMiddleware>();
        }
    }

    private class CorrelateStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelate(options =>
            {
                options.IncludeInResponse = true;
                options.RequestHeaders = new[]
                {
                    "X-Correlation-ID",
                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelate();
            app.UseMiddleware<DummyMiddleware>();
        }
    }
}
