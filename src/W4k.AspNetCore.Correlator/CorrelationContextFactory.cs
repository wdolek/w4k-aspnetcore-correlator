using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    internal class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly CorrelatorOptions _options;

        public CorrelationContextFactory(IOptions<CorrelatorOptions> options)
        {
            _options = options.Value;
        }

        public CorrelationContext CreateContext(HttpContext httpContext)
        {
            var knownHeaders = _options.ReadFrom;
            var generateCorrelationId = _options.Factory;

            var (headerName, headerValue) = GetCorrelationHeader(httpContext.Request.Headers, knownHeaders);

            return headerName is null
                ? generateCorrelationId is null
                    ? CorrelationContext.Empty
                    : CorrelationContext.FromFactory(knownHeaders.First(), generateCorrelationId(httpContext))
                : CorrelationContext.FromRequest(headerName, CorrelationId.FromString(headerValue));
        }

        private static (string? HeaderName, string? HeaderValue) GetCorrelationHeader(
            IHeaderDictionary headers,
            SortedSet<string> readFrom)
        {
            if (headers.Count == 0)
            {
                return default;
            }

            foreach (var keyValuePair in headers)
            {
                // NB! Given `SortedSet<>` should have ordinal & ignore case comparer for its elements
                if (!readFrom.Contains(keyValuePair.Key))
                {
                    continue;
                }

                if (keyValuePair.Value.Count > 0)
                {
                    return (keyValuePair.Key, keyValuePair.Value[0]);
                }
            }

            return default;
        }
    }
}
