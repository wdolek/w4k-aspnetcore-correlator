using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="IHeaderDictionary"/>.
    /// </summary>
    internal static class HeaderDictionaryExtensions
    {
        /// <summary>
        /// Gets name of HTTP header containing correlation ID (based on possible headers to choose from).
        /// </summary>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="readFrom">Enumerable of possible headers containing correlation ID.</param>
        /// <returns>
        /// Returns HTTP header name or <c>null</c> if header not found.
        /// </returns>
        public static string? GetCorrelationHeaderName(this IHeaderDictionary headers, IEnumerable<string> readFrom) =>
            readFrom
                .Intersect(headers.Keys, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(h => headers.Count > 0 && !string.IsNullOrEmpty(headers[h][0]));

        /// <summary>
        /// Gets correlation ID from HTTP request.
        /// </summary>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="headerName">Correlation ID header name.</param>
        /// <returns>
        /// Correlation ID if set, <see cref="CorrelationId.Empty"/> if header is missing or has invalid value.
        /// </returns>
        public static CorrelationId GetCorrelationId(this IHeaderDictionary headers, string? headerName)
        {
            if (string.IsNullOrEmpty(headerName) || !headers.ContainsKey(headerName))
            {
                return CorrelationId.Empty;
            }

            string? rawValue = headers[headerName].FirstOrDefault();

            return CorrelationId.FromString(rawValue);
        }

        /// <summary>
        /// Adds header value if not already set.
        /// </summary>
        /// <param name="headers">HTTP header dictionary.</param>
        /// <param name="headerName">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>
        /// Returns HTTP headers with header set.
        /// </returns>
        public static IHeaderDictionary AddHeaderIfNotSet(this IHeaderDictionary headers, string headerName, string value)
        {
            if (!headers.ContainsKey(headerName))
            {
                headers.Add(headerName, new StringValues(value));
            }

            return headers;
        }
    }
}
