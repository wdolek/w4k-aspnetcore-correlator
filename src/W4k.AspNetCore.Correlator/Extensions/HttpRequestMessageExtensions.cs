using System.Net.Http;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// HTTP request message extensions.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Adds header to request if not set.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="headerName">HTTP header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>
        /// HTTP request message with header set.
        /// </returns>
        public static HttpRequestMessage AddHeaderIfNotSet(
            this HttpRequestMessage request,
            string headerName,
            string value)
        {
            if (request == null
                || string.IsNullOrEmpty(headerName)
                || string.IsNullOrEmpty(value)
                || request.Headers.Contains(headerName))
            {
                return request;
            }

            request.Headers.Add(headerName, value);

            return request;
        }
    }
}
