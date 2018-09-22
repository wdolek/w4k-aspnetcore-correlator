using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public sealed class CorrelatorWithDefaultOptionsTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public CorrelatorWithDefaultOptionsTests()
        {
            IWebHostBuilder builder = new WebHostBuilder()
                .UseEnvironment("test")
                .UseStartup<StartupWithDefaultOptions>();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        public void Dispose()
        {
            _server?.Dispose();
            _client?.Dispose();
        }

        [Fact]
        public async Task CorrelationIdReadFromRequest()
        {
            // arrange: X-CorrelationId sent
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-CorrelationId", "123");

            // act
            HttpResponseMessage response = await _client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-CorrelationId").FirstOrDefault();

            // assert
            Assert.NotNull(correlationId);
            Assert.Equal("123", correlationId);
        }

        [Fact]
        public async Task CorrelationIdGenerated()
        {
            // arrange: no header sent
            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // act
            HttpResponseMessage response = await _client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-CorrelationId").FirstOrDefault();

            // assert
            Assert.NotNull(correlationId);
            Assert.True(Guid.TryParse(correlationId, out Guid _));
        }
    }
}
