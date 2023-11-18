using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace W4k.AspNetCore.Correlator;

public abstract class CorrelatorTestsBase<TStartup> : IDisposable
    where TStartup : class
{
    private readonly TestServer _server;
    private readonly Lazy<HttpClient> _client;

    protected CorrelatorTestsBase()
        : this(CreateTestWebHostBuilder())
    {
    }

    protected CorrelatorTestsBase(IWebHostBuilder builder)
    {
        _server = new TestServer(builder);
        _client = new Lazy<HttpClient>(() => _server.CreateClient());
    }

    protected HttpClient Client => _client.Value;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Client.Dispose();
            _server.Dispose();
        }
    }

    private static IWebHostBuilder CreateTestWebHostBuilder() =>
        new WebHostBuilder().UseStartup<TStartup>();
}