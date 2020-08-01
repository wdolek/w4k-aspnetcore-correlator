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
        /// Adds header value if not already set.
        /// </summary>
        /// <param name="headers">HTTP header dictionary.</param>
        /// <param name="headerName">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>
        /// Returns HTTP headers with header set.
        /// </returns>
        public static IHeaderDictionary AddHeaderIfNotSet(this IHeaderDictionary headers, string? headerName, string value)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                return headers;
            }

            if (!headers.ContainsKey(headerName))
            {
                headers.Add(headerName, new StringValues(value));
            }

            return headers;
        }
    }
}
