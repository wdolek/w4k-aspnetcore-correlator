using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Default implementation of correlation context factory.
    /// </summary>
    internal class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly CorrelatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class.
        /// </summary>
        /// <param name="options">Correlator options.</param>
        public CorrelationContextFactory(IOptions<CorrelatorOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc/>
        /// <returns>
        /// Correlation context, if not found and no factory method is configured,
        /// <see cref="EmptyCorrelationContext"/> is returned.
        /// </returns>
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

        /// <summary>
        /// Reads correlation ID HTTP header.
        /// </summary>
        /// <param name="headers">HTTP headers dictionary.</param>
        /// <param name="readFrom">Predefined header names to read value from.</param>
        /// <returns>
        /// Tuple containing either both header name and its value or tuple of <c>null</c> values.
        /// </returns>
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
