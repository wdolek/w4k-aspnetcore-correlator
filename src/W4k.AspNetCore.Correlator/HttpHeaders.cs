namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// HTTP header names.
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// <c>X-CorrelationId</c> header name.
        /// </summary>
        public static readonly string CorrelationId = "X-Correlation-ID";

        /// <summary>
        /// <c>X-RequestId</c> header name.
        /// </summary>
        public static readonly string RequestId = "X-Request-ID";
    }
}
