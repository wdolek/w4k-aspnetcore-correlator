using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Http;
using Xunit;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public sealed class CorrelatorWithDefaultOptionsTests : CorrelatorTestsBase<StartupWithDefaultOptions>
    {
        [Theory]
        [InlineData("Request-Id")]
        [InlineData("X-Correlation-ID")]
        [InlineData("x-correlation-id")]
        [InlineData("X-Request-ID")]
        public async Task CorrelationIdReadFromRequest(string correlationHeaderName)
        {
            // arrange: correlation ID header sent
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(correlationHeaderName, "123");

            // act: `X-Test-CorrelationId` is set by inline test middleware
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-Correlation-ID").FirstOrDefault();

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
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-Correlation-ID").FirstOrDefault();

            // assert
            Assert.NotNull(correlationId);
            Assert.True(Guid.TryParse(correlationId, out Guid _));
        }

        [Fact]
        public async Task CorrelationIdNotEmitted()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(HttpHeaders.CorrelationId, "123");

            // act: `X-Test-CorrelationId` is set by inline test middleware
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            Assert.False(response.Headers.Contains(HttpHeaders.CorrelationId));
        }
    }
}
