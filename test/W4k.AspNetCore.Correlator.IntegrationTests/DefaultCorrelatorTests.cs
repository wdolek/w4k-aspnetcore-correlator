using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;
using Xunit;

namespace W4k.AspNetCore.Correlator
{
    public class DefaultCorrelatorTests : CorrelatorTestsBase<DefaultOptionsStartup>
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

        [Fact]
        public async Task CorrelationIdNotFound()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-Dummy-Correlation-Id", "123");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            Assert.False(response.Headers.Contains("X-Dummy-Correlation-Id"));

            string correlationId = await response.Content.ReadAsStringAsync();
            Assert.NotEqual("123", correlationId);
            Assert.True(Guid.TryParse(correlationId, out Guid _));
        }
    }
}
