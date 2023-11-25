using System;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class CorrelationIdValueSanitizer
{
    // let's be generous here: 64 chars should be enough for correlation ID
    private const int MaxValueLength = 64;
    private const char SanitizedChar = '_';

#if NET8_0_OR_GREATER
    private static readonly System.Buffers.SearchValues<char> SafeCorrelationIdChars =
        System.Buffers.SearchValues.Create("!#$&+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ^_`abcdefghijklmnopqrstuvwxyz|~");
#endif

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

#if NET8_0_OR_GREATER
        var firstUnsafeCharPosition = valueSpan.IndexOfAnyExcept(SafeCorrelationIdChars);
        if (firstUnsafeCharPosition >= 0)
        {
            return SanitizeToNewString(value, valueLength, firstUnsafeCharPosition);
        }
#else
        for (int i = 0; i < valueLength; i++)
        {
            if (IsUnsafeChar(value[i]))
            {
                return SanitizeToNewString(value, valueLength, i);
            }
        }
#endif

        return value.Length > MaxValueLength
            ? valueSpan.ToString()
            : value;
    }

    // NB! we can't pass `ReadOnlySpan<char>` as state (as it's ref struct), see: https://github.com/dotnet/runtime/issues/30175
    private static string SanitizeToNewString(string source, int length, int firstUnsafeCharPosition) =>
        string.Create(length, (firstUnsafeCharPosition, source), CreateValue);

#if NET8_0_OR_GREATER
    private static void CreateValue(Span<char> buffer, (int FirstUnsafeCharPos, string SourceValue) state)
    {
        (int sourceIndex, string source) = state;

        // copy all safe chars before first unsafe char
        source
            .AsSpan(0, sourceIndex)
            .CopyTo(buffer);

        buffer[sourceIndex] = SanitizedChar;
        ++sourceIndex;

        // jump to next unsafe char, copy all safe chars between
        while (sourceIndex < buffer.Length)
        {
            var remainingSpan = source.AsSpan(sourceIndex);
            var nextUnsafeCharPos = remainingSpan.IndexOfAnyExcept(SafeCorrelationIdChars);

            // no more unsafe characters, copy remaining chars and break
            if (nextUnsafeCharPos == -1)
            {
                remainingSpan.CopyTo(buffer.Slice(sourceIndex));
                break;
            }

            remainingSpan
                .Slice(0, nextUnsafeCharPos)
                .CopyTo(buffer.Slice(sourceIndex));

            buffer[sourceIndex + nextUnsafeCharPos] = SanitizedChar;
            sourceIndex += nextUnsafeCharPos + 1;
        }
    }
#else
    private static void CreateValue(Span<char> buffer, (int FirstUnsafeCharPos, string SourceValue) state)
    {
        (int firstUnsafeCharPosition, string source) = state;

        source.AsSpan(0, firstUnsafeCharPosition).CopyTo(buffer);
        for (int i = firstUnsafeCharPosition; i < buffer.Length; i++)
        {
            var c = source[i];
            buffer[i] = IsUnsafeChar(c)
                ? SanitizedChar
                : c;
        }
    }

    private static bool IsUnsafeChar(char c)
    {
        if (c <= ' ' || c >= '~')
        {
            return true;
        }

        if (char.IsLetterOrDigit(c))
        {
            return false;
        }

        return c == '"'
            || c == '%'
            || c is >= '\'' and <= '*'
            || c == ','
            || c == '?'
            || c == '@'
            || c == '<'
            || c == '>'
            || c == '{'
            || c == '}';
    }
#endif
}