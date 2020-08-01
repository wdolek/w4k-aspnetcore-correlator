namespace W4k.AspNetCore.Correlator.Context.Types
{
    /// <summary>
    /// Empty correlation context - no correlation ID received with request and no ID has been generated.
    /// </summary>
    public sealed class EmptyCorrelationContext : CorrelationContext
    {
        /// <summary>
        /// Instance of <see cref="EmptyCorrelationContext"/>.
        /// </summary>
        public static readonly EmptyCorrelationContext Instance = new EmptyCorrelationContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyCorrelationContext"/> class.
        /// </summary>
        private EmptyCorrelationContext()
            : base(CorrelationId.Empty)
        {
        }
    }
}
