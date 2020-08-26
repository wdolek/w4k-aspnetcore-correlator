﻿using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;
using Xunit;

namespace W4k.AspNetCore.Correlator
{
    public sealed class CustomCorrelatorTests : CorrelatorTestsBase<CustomOptionsStartup>
    {
        [Fact]
        public async Task CorrelationIdReadFromRequest()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // incoming header name is customized in startup
            request.Headers.Add("X-CID", "123");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            Assert.True(response.Headers.Contains("X-CID"));

            string correlationIdEmitted = response.Headers.GetValues("X-CID").FirstOrDefault();
            Assert.Equal("123", correlationIdEmitted);

            string correlationId = await response.Content.ReadAsStringAsync();
            Assert.Equal("123", correlationId);
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
            Assert.Equal("", correlationId);
        }
    }
}