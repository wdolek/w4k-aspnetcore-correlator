using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace W4k.AspNetCore.Correlator.Http.Extensions;

internal static class HeaderDictionaryExtensions
{
    public static IHeaderDictionary AddHeaderIfNotSet(this IHeaderDictionary headers, string? headerName, string value)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            return headers;
        }

        headers.TryAdd(headerName, new StringValues(value));

        return headers;
    }
}
