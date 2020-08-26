namespace W4k.AspNetCore.Correlator.Http
{
    /// <summary>
    /// HTTP header names.
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// <c>X-CorrelationId</c> header name.
        /// </summary>
        public static readonly string CorrelationId = "X-Correlation-Id";

        /// <summary>
        /// <c>X-RequestId</c> header name.
        /// </summary>
        public static readonly string RequestId = "X-Request-Id";

        /// <summary>
        /// <c>Request-Id</c> header name.
        /// </summary>
        /// <remarks>
        /// Used by ASP.NET Core 2.x.
        /// </remarks>
        public static readonly string AspNetRequestId = "Request-Id";
    }
}
