using System.Net.Http;
using System.Net.Http.Headers;
using W4k.AspNetCore.Correlator.Http.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HttpRequestHeadersExtensionsTests
    {
        [Fact]
        public void AddIfNotSet_ExpectHeaderToBeAdded()
        {
            // arrange
            // (NB! there's no public ctor for `HttpRequestHeaders`)
            HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

            // act
            headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "123");

            // assert
            Assert.True(headers.Contains("X-Correlation-ID"));
            Assert.Contains("123", headers.GetValues("X-Correlation-ID"));
        }

        [Fact]
        public void AddIfNotSet_HeaderAlreadySet_ExpectKeepingOldValue()
        {
            // arrange
            HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;
            headers.Add("X-Correlation-ID", "123");

            // act
            // (try to set correlation ID "999")
            headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "999");

            // assert
            Assert.True(headers.Contains("X-Correlation-ID"));
            Assert.Contains("123", headers.GetValues("X-Correlation-ID"));
        }
    }
}
