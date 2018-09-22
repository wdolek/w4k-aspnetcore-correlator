using System;
using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Correlation ID extension methods.
    /// </summary>
    public static class CorrelationIdExtensions
    {
        /// <summary>
        /// Generates new correlation ID if provided one is empty.
        /// </summary>
        /// <param name="correlationId">Original correlation ID value.</param>
        /// <param name="factory">Correlation ID factory.</param>
        /// <returns>
        /// Either originally passed correlation ID or new one.
        /// </returns>
        public static CorrelationId GenerateIfEmpty(this CorrelationId correlationId, Func<CorrelationId> factory)
        {
            if (correlationId == CorrelationId.Empty && factory != null)
            {
                return factory.Invoke();
            }

            return correlationId;
        }

        /// <summary>
        /// Applies (sets) correlation ID to provided context if not empty.
        /// </summary>
        /// <remarks>
        /// If correlation ID is empty, it is not set to HTTP context.
        /// </remarks>
        /// <param name="correlationId">Correlation ID.</param>
        /// <param name="httpContext">HTTP context.</param>
        /// <returns>
        /// Correlation ID which has been set to context.
        /// </returns>
        public static CorrelationId ApplyTo(this CorrelationId correlationId, HttpContext httpContext)
        {
            if (httpContext == null)
            {
                return correlationId;
            }

            if (correlationId != CorrelationId.Empty)
            {
                httpContext.TraceIdentifier = correlationId;
            }

            return correlationId;
        }
    }
}
