using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.IntegrationTests.Startup;
using Xunit;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public sealed class DefaultCorrelatorTests : CorrelatorTestsBase<DefaultOptionsStartup>
    {
        [Theory]
        [InlineData("Request-Id")]
        [InlineData("X-Correlation-ID")]
        [InlineData("x-correlation-id")]
        [InlineData("X-Request-ID")]
        public async Task CorrelationIdReadFromRequest(string correlationHeaderName)
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(correlationHeaderName, "123");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            string correlationId = await response.Content.ReadAsStringAsync();
            Assert.Equal("123", correlationId);
        }

        [Fact]
        public async Task CorrelationIdGenerated()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            string correlationId = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(correlationId);
            Assert.True(Guid.TryParse(correlationId, out Guid _));
        }
    }
}
