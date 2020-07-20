using System.Net.Http.Headers;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="HttpRequestHeaders"/>.
    /// </summary>
    internal static class HttpRequestHeadersExtensions
    {
        /// <summary>
        /// Adds header value if not already set.
        /// </summary>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="headerName">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>
        /// Returns HTTP request headers with header set.
        /// </returns>
        public static HttpRequestHeaders AddHeaderIfNotSet(this HttpRequestHeaders headers, string headerName, string value)
        {
            if (!headers.Contains(headerName))
            {
                headers.Add(headerName, value);
            }

            return headers;
        }
    }
}
