using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using W4k.AspNetCore.Correlator.UnitTests.Misc;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public static class HttpRequestExtensions
    {
        public static Mock<HttpRequest> SetupEmptyHeaders(this Mock<HttpRequest> request)
        {
            request.Setup(r => r.Headers).Returns(new HeaderDictionary());

            return request;
        }

        public static Mock<HttpRequest> SetupRequestHeaders(
            this Mock<HttpRequest> request,
            string name,
            params string[] values)
        {
            request.Setup(r => r.Headers).Returns(
                new HeaderDictionary
                {
                    [name] = new StringValues(values)
                });

            return request;
        }
    }
}
