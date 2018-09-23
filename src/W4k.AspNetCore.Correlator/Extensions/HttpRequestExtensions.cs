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
        /// Returns tuple of correlation ID and header name from which value was read.
        /// If header is not set or contains invalid value, <see cref="CorrelationId.Empty"/> is returned
        /// with <c>null</c> header value.
        /// </returns>
        public static (CorrelationId CorrelationId, string HeaderName) ReadCorrelationId(
            this HttpRequest request,
            IEnumerable<string> fromHeaders)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (fromHeaders == null)
            {
                throw new ArgumentNullException(nameof(fromHeaders));
            }

            string headerName = null;
            string value = null;
            foreach (string header in fromHeaders)
            {
                if (request.Headers.TryGetValue(header, out StringValues headerValues)
                    && headerValues.Count > 0
                    && !string.IsNullOrEmpty(headerValues[0]))
                {
                    headerName = header;
                    value = headerValues[0];
                    break;
                }
            }

            // NB! correlation ID may be empty (but still something), however header name may be null
            return (CorrelationId.FromString(value) ?? CorrelationId.Empty, headerName);
        }
    }
}
