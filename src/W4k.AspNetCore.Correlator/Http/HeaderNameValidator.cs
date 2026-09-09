using System;
using System.Buffers;

namespace W4k.AspNetCore.Correlator.Http;

internal static class HeaderNameValidator
{
    // RFC 7230 token characters (tchar)
    private const string TokenChars = "!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly SearchValues<char> TokenCharSet = SearchValues.Create(TokenChars);

    public static bool IsValidHeaderName(string? headerName) =>
        !string.IsNullOrEmpty(headerName) && headerName.AsSpan().IndexOfAnyExcept(TokenCharSet) < 0;
}
