using System;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class CorrelationIdValueSanitizer
{
    internal const int MaxValueLength = 80;
    internal const char SanitizedChar = '*';

    internal const string SafeCorrelationIdCharsString = "#+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz|~";

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

        var firstUnsafeCharPosition = valueSpan.IndexOfAnyExcept(SafeCorrelationIdChars);
        if (firstUnsafeCharPosition < 0)
        {
            return value.Length > MaxValueLength
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

            buffer[unsafeCharIdx] = SanitizedChar;
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

    private static readonly System.Buffers.SearchValues<char> SafeCorrelationIdChars =
        System.Buffers.SearchValues.Create(SafeCorrelationIdCharsString);

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
