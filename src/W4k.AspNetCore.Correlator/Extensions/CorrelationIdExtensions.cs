using System;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="CorrelationId"/>.
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
    }
}
