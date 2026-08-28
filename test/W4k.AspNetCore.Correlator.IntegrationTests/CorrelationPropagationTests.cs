using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator;

public sealed class CorrelationPropagationTests
{
    private IHost _hostAlpha = null!;
    private IHost _hostBeta = null!;

    [Before(Test)]
    public async Task SetUpTestHosts()
    {
        _hostAlpha = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenLocalhost(8081);
                });
                webBuilder.UseStartup<StartupAlpha>();
            })
            .Build();

        _hostBeta = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenLocalhost(8082);
                });
                webBuilder.UseStartup<StartupBeta>();
            })
            .Build();

        await _hostAlpha.StartAsync();
        await _hostBeta.StartAsync();
    }

    [Test]
    public async Task ForwardCorrelation_WhenChangingHeader_ExpectCorrelationSent()
    {
        // arrange
        var correlation = "test-123";
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8081/")
        {
            Headers =
            {
                { "X-Correlation-Id", correlation }
            }
        };

        // act
        //
        // <test> (X-Correlation-Id: test-123)
        //      --> host A (X-Request-Id: test-123)
        //          --> host B
        //
        // 1. send request to Host A, X-Correlation-Id: test-123
        // 2. send request from Host A to Host B, X-Request-Id: test-123
        // 3. cascade result from Host B
        using var httpClient = new HttpClient();

        var response = await httpClient.SendAsync(request);
        var receivedValue = await response.Content.ReadAsStringAsync();

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(receivedValue).IsEqualTo($"X-Request-Id:{correlation}");
    }

    [After(Test)]
    public async Task TearDownTestHosts()
    {
        if (_hostAlpha is not null)
        {
            await _hostAlpha.StopAsync();
            _hostAlpha.Dispose();
        }

        if (_hostBeta is not null)
        {
            await _hostBeta.StopAsync();
            _hostBeta.Dispose();
        }
    }

    private class StartupAlpha
    {
        private const string BetaClientName = "TestBetaClient";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(o =>
            {
                o.Forward = PropagationSettings.PropagateAs("X-Request-Id");
            });

            services
                .AddHttpClient(
                    BetaClientName,
                    httpClient => httpClient.BaseAddress = new Uri("http://localhost:8082"))
                .WithCorrelation();
        }

        public void Configure(IApplicationBuilder app, IHttpClientFactory httpClientFactory)
        {
            app.UseCorrelator();
            app.Run(async (context) =>
            {
                var response = await httpClientFactory.CreateClient(BetaClientName).GetAsync("/");
                var receivedValue = await response.Content.ReadAsStringAsync();

                context.Response.Headers.Append("Content-Type", "text/plain");
                await context.Response.WriteAsync(receivedValue);
            });
        }
    }

    private class StartupBeta
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator();
        }

        public void Configure(IApplicationBuilder app, ICorrelationContextAccessor correlationContextAccessor)
        {
            app.UseCorrelator();
            app.Run(async (context) =>
            {
                context.Response.Headers.Append("Content-Type", "text/plain");

                var correlationContext = (RequestCorrelationContext)correlationContextAccessor.CorrelationContext;
                await context.Response.WriteAsync($"{correlationContext.Header}:{correlationContext.CorrelationId}");
            });
        }
    }
}