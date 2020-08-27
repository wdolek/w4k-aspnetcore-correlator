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
            IHeaderDictionary requestHeaders,
            ICollection<string> expectedHeaders)
        {
            if (requestHeaders.Count == 0)
            {
                return default;
            }

            // lookup expected header in request headers
            // (expecting that there are going to be more request headers than we have configured for expectation)
            foreach (var header in expectedHeaders)
            {
                if (!requestHeaders.ContainsKey(header))
                {
                    continue;
                }

                var values = requestHeaders[header];
                if (values.Count > 0 && !string.IsNullOrEmpty(values[0]))
                {
                    return (header, values[0]);
                }
            }

            return default;
        }
    }
}
