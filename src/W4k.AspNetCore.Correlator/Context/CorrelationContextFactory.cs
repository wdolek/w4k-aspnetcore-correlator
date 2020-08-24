using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Context
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
            var (headerName, headerValue) = GetCorrelationHeader(httpContext.Request.Headers, _options.ReadFrom);

            if (headerName is null)
            {
                var generateCorrelationId = _options.Factory;

                return generateCorrelationId is null
                    ? EmptyCorrelationContext.Instance
                    : (CorrelationContext)new GeneratedCorrelationContext(generateCorrelationId(httpContext));
            }

            return new RequestCorrelationContext(CorrelationId.FromString(headerValue), headerName);
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
