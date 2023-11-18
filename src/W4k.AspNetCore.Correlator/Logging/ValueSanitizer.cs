using System;
using System.Runtime.CompilerServices;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class ValueSanitizer
{
    // let's be generous here: 256 chars should be enough for correlation ID; apart of that, we do sanitization to avoid log injection
    private const int MaxValueLength = 256;
    private const char SanitizedChar = '_';

    public static string Sanitize(string value)
    {
        var maxValueLength = Math.Min(value.Length, MaxValueLength);
        var source = value.AsSpan(0, maxValueLength);

        for (int i = 0; i < maxValueLength; i++)
        {
            char currentChar = source[i];
            if (!char.IsLetterOrDigit(currentChar))
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

    [SkipLocalsInit]
    private static string SanitizeWithAllocation(ReadOnlySpan<char> source, int maxValueLength)
    {
        Span<char> destination = stackalloc char[maxValueLength];

        for (int i = 0; i < maxValueLength; i++)
        {
            var currentChar = source[i];
            destination[i] = char.IsLetterOrDigit(currentChar)
                ? currentChar
                : SanitizedChar;
        }

        return destination.ToString();
    }
}