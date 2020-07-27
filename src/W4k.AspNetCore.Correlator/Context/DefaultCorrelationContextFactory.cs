using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Context
{
    internal class DefaultCorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly CorrelatorOptions _options;

        public DefaultCorrelationContextFactory(IOptions<CorrelatorOptions> options)
        {
            _options = options.Value;
        }

        public CorrelationContext CreateContext(HttpContext httpContext)
        {
            var (headerName, headerValue) = GetCorrelationHeader(httpContext.Request.Headers, _options.ReadFrom);

            return (headerName, headerValue, _options.Factory) switch
            {
                // correlation ID received
                (string h, string v, _) =>
                    new RequestCorrelationContext(CorrelationId.FromString(v), h),

                // correlation ID not received, to be generated
                (null, null, Func<HttpContext, CorrelationId> f) =>
                    new GeneratedCorrelationContext(f(httpContext)),

                // nop
                _ => EmptyCorrelationContext.Instance,
            };
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
