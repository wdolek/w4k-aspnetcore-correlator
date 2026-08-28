using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace W4k.AspNetCore.Correlator;

public abstract class CorrelatorTestsBase<TStartup>
    where TStartup : class
{
    private IHost _host = null!;
    private Lazy<HttpClient> _client = null!;

    protected CorrelatorTestsBase()
    {
    }

    protected CorrelatorTestsBase(IHostBuilder builder)
    {
        Builder = builder;
    }

    protected IHostBuilder? Builder { get; }

    protected HttpClient Client => _client.Value;

    [Before(Test)]
    public async Task SetUpTestHost()
    {
        _host = (Builder ?? CreateTestWebHostBuilder()).Build();
        await _host.StartAsync();

        _client = new Lazy<HttpClient>(() => _host.GetTestServer().CreateClient());
    }

    [After(Test)]
    public async Task TearDownTestHost()
    {
        if (_client is not null && _client.IsValueCreated)
        {
            Client.Dispose();
        }

        if (_host is not null)
        {
            await _host.StopAsync();
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