using System;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class CorrelationIdValueSanitizer
{
    internal const int MaxValueLength = 80;
    internal const char SanitizedChar = '*';

    internal const string SafeCorrelationIdCharsString = "#+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz|~";

    public static string Sanitize(string value) =>
#if NET8_0
        CorrelationIdValueSanitizerNet8.Sanitize(value);
#elif NET9_0_OR_GREATER
        CorrelationIdValueSanitizerNet9.Sanitize(value);
#endif
}

#if NET8_0
file static class CorrelationIdValueSanitizerNet8
{
    private static readonly System.Buffers.SearchValues<char> SafeCorrelationIdChars =
        System.Buffers.SearchValues.Create(CorrelationIdValueSanitizer.SafeCorrelationIdCharsString);

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, CorrelationIdValueSanitizer.MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

        var firstUnsafeCharPosition = valueSpan.IndexOfAnyExcept(SafeCorrelationIdChars);
        if (firstUnsafeCharPosition >= 0)
        {
            return SanitizeToNewString(value, valueLength, firstUnsafeCharPosition);
        }

        return value.Length > CorrelationIdValueSanitizer.MaxValueLength
            ? valueSpan.ToString()
            : value;
    }

    // NB! we can't pass `ReadOnlySpan<char>` as state (as it's ref struct), see: https://github.com/dotnet/runtime/issues/30175
    private static string SanitizeToNewString(string source, int length, int firstUnsafeCharPosition) =>
        string.Create(length, (firstUnsafeCharPosition, source), CreateValue);

    private static void CreateValue(Span<char> buffer, (int FirstUnsafeCharPos, string SourceValue) state)
    {
        var (sourceIndex, source) = state;

        // copy all safe chars before first unsafe char
        source
            .AsSpan(0, sourceIndex)
            .CopyTo(buffer);

        buffer[sourceIndex] = CorrelationIdValueSanitizer.SanitizedChar;
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

            buffer[sourceIndex + nextUnsafeCharPos] = CorrelationIdValueSanitizer.SanitizedChar;
            sourceIndex += nextUnsafeCharPos + 1;
        }
    }
}
#endif

#if NET9_0_OR_GREATER
file static class CorrelationIdValueSanitizerNet9
{
    private static readonly System.Buffers.SearchValues<char> SafeCorrelationIdChars =
        System.Buffers.SearchValues.Create(CorrelationIdValueSanitizer.SafeCorrelationIdCharsString);

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, CorrelationIdValueSanitizer.MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

        var firstUnsafeCharPosition = valueSpan.IndexOfAnyExcept(SafeCorrelationIdChars);
        if (firstUnsafeCharPosition < 0)
        {
            return value.Length > CorrelationIdValueSanitizer.MaxValueLength
                ? valueSpan.ToString()
                : value;
        }

        return string.Create(valueLength, new SanitizeState(valueSpan, firstUnsafeCharPosition), CreateValue);
    }

    private static void CreateValue(Span<char> buffer, SanitizeState state)
    {
        var source = state.Source;

        var currentIdx = 0;
        var unsafeCharIdx = state.UnsafeCharIndex;

        do
        {
            source
                .Slice(currentIdx, unsafeCharIdx - currentIdx)
                .CopyTo(buffer.Slice(currentIdx));

            buffer[unsafeCharIdx] = CorrelationIdValueSanitizer.SanitizedChar;
            currentIdx = unsafeCharIdx + 1;

            unsafeCharIdx =
                source
                    .Slice(currentIdx)
                    .IndexOfAnyExcept(SafeCorrelationIdChars);

            unsafeCharIdx = unsafeCharIdx < 0
                ? buffer.Length
                : unsafeCharIdx + currentIdx;
        }
        while (unsafeCharIdx < buffer.Length);

        // copy remaining chars
        if (currentIdx < buffer.Length)
        {
            source
                .Slice(currentIdx)
                .CopyTo(buffer.Slice(currentIdx));
        }
    }

    private readonly ref struct SanitizeState
    {
        public SanitizeState(ReadOnlySpan<char> source, int unsafeCharIndex)
        {
            UnsafeCharIndex = unsafeCharIndex;
            Source = source;
        }

        public int UnsafeCharIndex { get; }
        public ReadOnlySpan<char> Source { get; }
    }
}
#endif
