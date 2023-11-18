using System.Net.Http.Headers;

namespace W4k.AspNetCore.Correlator.Http.Extensions;

internal static class HttpRequestHeadersExtensions
{
    public static HttpRequestHeaders AddHeaderIfNotSet(this HttpRequestHeaders headers, string? headerName, string? value)
    {
        if (string.IsNullOrEmpty(headerName) || string.IsNullOrEmpty(value))
        {
            return headers;
        }

        if (!headers.Contains(headerName))
        {
            headers.Add(headerName, value);
        }

        return headers;
    }
}