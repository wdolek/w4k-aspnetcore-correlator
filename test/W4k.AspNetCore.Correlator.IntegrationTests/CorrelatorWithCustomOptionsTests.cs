﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public sealed class CorrelatorWithCustomOptionsTests : CorrelatorTestsBase<StartupWithCustomOptions>
    {
        [Fact]
        public async Task CorrelationIdReadFromRequest()
        {
            // arrange: correlation ID header sent
            // (read from is customized in startup)
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-CID", "123");

            // act: `X-Test-CorrelationId` is set by inline test middleware
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-Correlation-ID").FirstOrDefault();

            // assert
            Assert.NotNull(correlationId);
            Assert.Equal("123", correlationId);
        }

        [Fact]
        public async Task CorrelationIdNotFound()
        {
            // arrange: unrecognized correlation ID header
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-CorrelationId", "123");

            // act: `X-Test-CorrelationId` is set by inline test middleware
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-Correlation-ID").FirstOrDefault();

            // assert
            // - correlation ID is generated by ASP.NET
            // - correlation ID does not equal to value we sent
            Assert.NotNull(correlationId);
            Assert.NotEqual("123", correlationId);
        }

        [Fact]
        public async Task CorrelationIdNotGenerated()
        {
            // arrange: no header sent
            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);
            string correlationId = response.Headers.GetValues("X-Test-Correlation-ID").FirstOrDefault();

            // assert
            // - correlation ID is generated by ASP.NET
            // - correlation ID does not have form of Guid
            Assert.NotNull(correlationId);
            Assert.False(Guid.TryParse(correlationId, out Guid _));
        }
    }
}
