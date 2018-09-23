namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Header propagation types.
    /// </summary>
    public enum HeaderPropagation
    {
        /// <summary>
        /// No correlation ID header propagation.
        /// </summary>
        NoPropagation = 0,

        /// <summary>
        /// Use predefined header name for propagation.
        /// </summary>
        UsePredefinedHeaderName,

        /// <summary>
        /// Use same header name for correlation ID as received.
        /// </summary>
        KeepIncomingHeaderName,
    }
}
