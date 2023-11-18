namespace W4k.AspNetCore.Correlator.Http;

/// <summary>
/// HTTP header names.
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// <c>X-Correlation-Id</c> header name.
    /// </summary>
    public static readonly string CorrelationId = "X-Correlation-Id";

    /// <summary>
    /// <c>X-Request-Id</c> header name.
    /// </summary>
    public static readonly string RequestId = "X-Request-Id";

    /// <summary>
    /// <c>Request-Id</c> header name.
    /// </summary>
    public static readonly string AspNetRequestId = "Request-Id";
}