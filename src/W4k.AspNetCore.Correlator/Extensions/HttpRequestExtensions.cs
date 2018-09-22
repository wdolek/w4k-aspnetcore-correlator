using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// HTTP request extension methods.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Read correlation ID from HTTP request.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <param name="fromHeaders">Enumerable of HTTP header names to be used for correlation ID.</param>
        /// <returns>
        /// Correlation ID read from request headers or empty correlation ID if header(s) not set.
        /// </returns>
        public static CorrelationId ReadCorrelationId(this HttpRequest request, IEnumerable<string> fromHeaders)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (fromHeaders == null)
            {
                throw new ArgumentNullException(nameof(fromHeaders));
            }

            string value = null;
            foreach (string header in fromHeaders)
            {
                if (request.Headers.TryGetValue(header, out StringValues headerValues)
                    && headerValues.Count > 0
                    && !string.IsNullOrEmpty(headerValues[0]))
                {
                    value = headerValues[0];
                    break;
                }
            }

            return CorrelationId.FromString(value) ?? CorrelationId.Empty;
        }
    }
}
