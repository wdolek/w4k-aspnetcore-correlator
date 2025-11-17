using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace W4k.AspNetCore.Correlator;

public abstract class CorrelatorTestsBase<TStartup> : IDisposable
    where TStartup : class
{
    private readonly IHost _host;
    private readonly Lazy<HttpClient> _client;

    protected CorrelatorTestsBase()
        : this(CreateTestWebHostBuilder())
    {
    }

    protected CorrelatorTestsBase(IHostBuilder builder)
    {
        _host = builder.Build();
        _host.Start();

        _client = new Lazy<HttpClient>(() => _host.GetTestServer().CreateClient());
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
            _host.Dispose();
        }
    }

    private static IHostBuilder CreateTestWebHostBuilder() =>
        new HostBuilder().ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder
                .UseTestServer()
                .UseStartup<TStartup>();
        });
}