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
        /// Either originally passed correlation ID or newly generated one (if <paramref name="factory"/> set, otherwise <see cref="CorrelationId.Empty"/>).
        /// </returns>
        public static CorrelationId GenerateIfEmpty(this CorrelationId correlationId, Func<CorrelationId>? factory) =>
            correlationId is object && !correlationId.IsEmpty
                ? correlationId
                : factory is null
                    ? CorrelationId.Empty
                    : factory.Invoke();
    }
}
