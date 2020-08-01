using System;

namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Base correlation context classs.
    /// </summary>
    public abstract class CorrelationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContext"/> class.
        /// </summary>
        /// <param name="correlationId">Correlation ID.</param>
        protected CorrelationContext(CorrelationId correlationId)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        }

        /// <summary>
        /// Gets correlation ID.
        /// </summary>
        public CorrelationId CorrelationId { get; }
    }
}
