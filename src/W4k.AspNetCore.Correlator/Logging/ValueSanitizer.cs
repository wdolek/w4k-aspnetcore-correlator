using System;
using System.Runtime.CompilerServices;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class ValueSanitizer
{
    // let's be generous here: despite HTTP specs don't limit header value length, 8KiB should be more than enough
    private const int MaxValueLength = 8 * 1024;
    private const char SanitizedChar = '_';

    [SkipLocalsInit]
    public static string Sanitize(string value)
    {
        var wasSanitized = false;
        var maxValueLength = value.Length;
        if (maxValueLength > MaxValueLength)
        {
            maxValueLength = MaxValueLength;
            wasSanitized = true;
        }

        var source = value.AsSpan(0, maxValueLength);
        Span<char> destination = stackalloc char[maxValueLength];

        for (int i = 0; i < maxValueLength; i++)
        {
            char currentChar = source[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                destination[i] = currentChar;
            }
            else
            {
                destination[i] = SanitizedChar;
                wasSanitized = true;
            }
        }

        // if there was no sanitization, we can return original value to avoid allocation
        // (stack-allocated memory is wasted in such case, but it's better than new heap allocation)
        return wasSanitized
            ? destination.ToString()
            : value;
    }
}