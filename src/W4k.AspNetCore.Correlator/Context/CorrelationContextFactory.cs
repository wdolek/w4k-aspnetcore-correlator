using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Logging;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Context
{
    internal class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly CorrelatorOptions _options;
        private readonly ILogger<CorrelationContextFactory> _logger;

        public CorrelationContextFactory(
            IOptions<CorrelatorOptions> options,
            ILogger<CorrelationContextFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public CorrelationContext CreateContext(HttpContext httpContext)
        {
            var (headerName, headerValue) = GetCorrelationHeader(httpContext.Request.Headers, _options.ReadFrom);
            if (headerName is null)
            {
                _logger.NoCorrelationHeaderReceived();

                var generateCorrelationId = _options.Factory;
                if (generateCorrelationId is null)
                {
                    _logger.NoCorrelationIdFactoryConfigured();
                    return EmptyCorrelationContext.Instance;
                }

                _logger.GeneratingCorrelationId();
                return new GeneratedCorrelationContext(generateCorrelationId(httpContext));
            }

            _logger.CorrelationIdReceived(headerName, headerValue!);
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
