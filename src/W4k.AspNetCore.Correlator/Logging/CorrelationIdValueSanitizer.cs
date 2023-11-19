using System;
using System.Runtime.CompilerServices;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class CorrelationIdValueSanitizer
{
    // let's be generous here: 64 chars should be enough for correlation ID
    private const int MaxValueLength = 64;
    private const char SanitizedChar = '_';

    public static string Sanitize(string value)
    {
        var maxValueLength = Math.Min(value.Length, MaxValueLength);
        var source = value.AsSpan(0, maxValueLength);

        for (int i = 0; i < maxValueLength; i++)
        {
            char currentChar = source[i];
            if (IsUnsafeChar(currentChar))
            {
                // break early, sanitize input value
                return SanitizeWithAllocation(source, maxValueLength);
            }
        }

        // no need for sanitization, but we need to truncate value
        if (value.Length > MaxValueLength)
        {
            return source.ToString();
        }

        // no need for sanitization
        return value;
    }

    private static bool IsUnsafeChar(char c)
    {
        if (char.IsLetterOrDigit(c))
        {
            return false;
        }

        return c < ' ' || c > 127 || c == '<' || c == '>' || c == '&' || c == '\'' || c == '\"';
    }

    [SkipLocalsInit]
    private static string SanitizeWithAllocation(ReadOnlySpan<char> source, int maxValueLength)
    {
        Span<char> destination = stackalloc char[maxValueLength];

        for (int i = 0; i < maxValueLength; i++)
        {
            var currentChar = source[i];
            destination[i] = IsUnsafeChar(currentChar)
                ? SanitizedChar
                : currentChar;
        }

        return destination.ToString();
    }
}