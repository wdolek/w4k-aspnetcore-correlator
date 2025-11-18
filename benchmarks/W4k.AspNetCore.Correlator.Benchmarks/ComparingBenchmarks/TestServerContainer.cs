using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace W4k.AspNetCore.Correlator.Benchmarks.ComparingBenchmarks;

internal class TestServerContainer : IDisposable
{
    private readonly IHost _host;

    private TestServerContainer(IHost host)
    {
        _host = host;
        _host.Start();

        HttpClient = _host.GetTestServer().CreateClient();
    }

    public HttpClient HttpClient { get; }

    public static TestServerContainer Create<TStartup>() where TStartup : class
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseTestServer()
                .UseStartup<TStartup>())
            .Build();

        return new TestServerContainer(host);
    }

    public void Dispose()
    {
        _host?.Dispose();
        HttpClient?.Dispose();
    }
}