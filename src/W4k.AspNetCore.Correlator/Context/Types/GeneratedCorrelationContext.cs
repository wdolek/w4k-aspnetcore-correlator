namespace W4k.AspNetCore.Correlator.Context.Types
{
    /// <summary>
    /// Correlation context with generated correlation ID.
    /// </summary>
    public sealed class GeneratedCorrelationContext : CorrelationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedCorrelationContext"/> class.
        /// </summary>
        /// <param name="correlationId">Generated correlation ID.</param>
        public GeneratedCorrelationContext(CorrelationId correlationId)
            : base(correlationId)
        {
        }
    }
}
