using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HttpRequestExtensionsShould
    {
        [Fact]
        public void ThrowOnNullRequest()
        {
            Assert.Throws<ArgumentNullException>(
                () => Correlator.Extensions.HttpRequestExtensions.ReadCorrelationId(
                    null,
                    Enumerable.Empty<string>()));
        }

        [Fact]
        public void ThrowOnNullFromHeaders()
        {
            Assert.Throws<ArgumentNullException>(
                () => Correlator.Extensions.HttpRequestExtensions.ReadCorrelationId(
                    new Mock<HttpRequest>().Object,
                    null));
        }

        [Theory]
        [MemberData(nameof(GenerateHttpRequests))]
        public void ReadExpectedCorrelationId(
            HttpRequest request,
            IEnumerable<string> fromHeaders,
            CorrelationId expected)
        {
            CorrelationId correlationId = request.ReadCorrelationId(fromHeaders);
            Assert.Equal(expected, correlationId);
        }

        public static IEnumerable<object[]> GenerateHttpRequests()
        {
            var headers = new List<string>
            {
                "X-CorrelationId",
                "X-RequestId"
            };

            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-CorrelationId", "123").Object,
                headers,
                CorrelationId.FromString("123").Value
            };
            
            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-RequestId", "123").Object,
                headers,
                CorrelationId.FromString("123").Value
            };

            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-CorrelationId", string.Empty).Object,
                headers,
                CorrelationId.Empty
            };

            yield return new object[]
            {
                new Mock<HttpRequest>().SetupEmptyHeaders().Object,
                headers,
                CorrelationId.Empty
            };
            
            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-Unrecognized", "123").Object,
                headers,
                CorrelationId.Empty
            };
            
            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-CorrelationId", "123", "456", "789").Object,
                headers,
                CorrelationId.FromString("123").Value
            };

            yield return new object[]
            {
                new Mock<HttpRequest>().SetupRequestHeaders("X-CorrelationId", "123").Object,
                Enumerable.Empty<string>(),
                CorrelationId.Empty
            };
        }
    }
}
