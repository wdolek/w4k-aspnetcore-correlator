using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// HTTP response extension methods.
    /// </summary>
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Sets HTTP header to provided response.
        /// </summary>
        /// <param name="response">HTTP response.</param>
        /// <param name="propagation">Correlation ID propagation settings.</param>
        /// <param name="incomingHeader">Name of incoming header.</param>
        /// <param name="correlationId">Correlation ID to emit.</param>
        public static bool TryEmitCorrelationId(
            this HttpResponse response,
            PropagationSettings propagation,
            string incomingHeader,
            CorrelationId correlationId)
        {
            if (propagation.Settings == HeaderPropagation.NoPropagation)
            {
                return false;
            }

            // NB! `incomingHeader` may be null if correlation ID not sent with request
            var headerName = propagation.Settings == HeaderPropagation.UsePredefinedHeaderName
                ? propagation.HeaderName
                : incomingHeader ?? HttpHeaders.CorrelationId;

            if (response.Headers.ContainsKey(headerName))
            {
                return false;
            }

            response.Headers.Add(headerName, new StringValues(correlationId));

            return true;
        }
    }
}
