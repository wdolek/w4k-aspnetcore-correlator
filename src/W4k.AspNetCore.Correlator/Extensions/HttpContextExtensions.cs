using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="HttpContext"/>.
    /// </summary>
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// Sets correlation ID to given HTTP context.
        /// </summary>
        /// <param name="httpContext">HTTP context to be enriched.</param>
        /// <param name="correlationId">Request correlation ID.</param>
        /// <returns>
        /// HTTP context with provided correlation ID (if not empty).
        /// </returns>
        public static HttpContext WithCorrelationId(this HttpContext httpContext, CorrelationId correlationId)
        {
            if (correlationId != CorrelationId.Empty)
            {
                httpContext.TraceIdentifier = correlationId;
            }

            return httpContext;
        }
    }
}
