namespace W4k.AspNetCore.Correlator.Context.Types
{
    /// <summary>
    /// Correlation context with correlation ID received from HTTP request.
    /// </summary>
    public sealed class RequestCorrelationContext : CorrelationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCorrelationContext"/> class.
        /// </summary>
        /// <param name="correlationId">Request correlation ID.</param>
        /// <param name="originHeader">Header name containing correlation ID.</param>
        public RequestCorrelationContext(CorrelationId correlationId, string originHeader)
            : base(correlationId)
        {
            ThrowHelper.ThrowIfNull(originHeader, nameof(originHeader));
            Header = originHeader;
        }

        /// <summary>
        /// Gets request header name which contained correlation ID value.
        /// </summary>
        public string Header { get; }
    }
}
