using System;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;

namespace W4k.AspNetCore.Correlator.Benchmarks.ComparingBenchmarks;

internal class TestServerContainer : IDisposable
{
    private readonly TestServer _server;

    public TestServerContainer(TestServer server)
    {
        _server = server;
        HttpClient = _server.CreateClient();
    }

    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        HttpClient?.Dispose();
        _server?.Dispose();
    }
}