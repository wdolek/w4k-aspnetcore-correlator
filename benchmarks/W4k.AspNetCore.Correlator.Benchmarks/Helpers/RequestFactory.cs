using System;
using System.Net.Http;
using System.Text;

namespace W4k.AspNetCore.Correlator.Benchmarks.Helpers
{
    internal static class RequestFactory
    {
        private const string JsonPayload = @"{ ""user"": ""Bob"" }";

        private static readonly string CorrelationId = Guid.NewGuid().ToString("D");

        public static HttpRequestMessage CreateCorrelatedRequest() =>
            new HttpRequestMessage(HttpMethod.Get, "/")
            {
                Content = new StringContent(JsonPayload, Encoding.UTF8, "application/json"),
                Headers =
                    {
                        { "Accept", "application/json" },
                        { "X-Correlation-Id", CorrelationId },
                    }
            };

        public static HttpRequestMessage CreateCorrelatedRequestWithAdditionalHeaders() =>
            new HttpRequestMessage(HttpMethod.Get, "/")
            {
                Content = new StringContent(JsonPayload, Encoding.UTF8, "application/json"),
                Headers =
                    {
                        { "Host", "localhost" },
                        { "Accept", "application/json" },
                        { "Accept-Language", "en_US" },
                        { "RequestId", "ASPNET_REQUEST_ID:123" },
                        { "Cache-Control", "no-cache" },
                        { "Origin", "http://localhost" },
                        { "Referer", "http://localhost" },
                        { "Forwarded", "for=127.0.0.1, for=127.0.0.1" },
                        { "X-Forwarded-For", "127.0.0.1" },
                        { "X-Correlation-Id", CorrelationId },
                    },
            };

        public static HttpRequestMessage CreateRequestWithoutCorrelation() =>
            new HttpRequestMessage(HttpMethod.Get, "/")
            {
                Content = new StringContent(JsonPayload, Encoding.UTF8, "application/json"),
                Headers =
                    {
                        { "Accept", "application/json" },
                    }
            };
    }
}
