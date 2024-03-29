﻿using System;

namespace W4k.AspNetCore.Correlator.Logging;

internal static class CorrelationIdValueSanitizer
{
    internal const int MaxValueLength = 80;
    internal const char SanitizedChar = '*';

    internal const string SafeCorrelationIdCharsString = "#+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz|~";

    public static string Sanitize(string value)
    {
#if NET8_0_OR_GREATER
        return CorrelationIdValueSanitizerNet8.Sanitize(value);
#else
        return CorrelationIdValueSanitizerNet6.Sanitize(value);
#endif
    }
}

#if NET8_0_OR_GREATER
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
        (int sourceIndex, string source) = state;

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

#if !NET8_0_OR_GREATER
file static class CorrelationIdValueSanitizerNet6
{
    private static readonly System.Collections.Generic.HashSet<char> SafeCorrelationIdChars =
        new(CorrelationIdValueSanitizer.SafeCorrelationIdCharsString);

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, CorrelationIdValueSanitizer.MaxValueLength);

        for (int charPosition = 0; charPosition < valueLength; charPosition++)
        {
            if (IsUnsafeChar(value[charPosition]))
            {
                return SanitizeToNewString(value, valueLength, charPosition);
            }
        }

        return value.Length > CorrelationIdValueSanitizer.MaxValueLength
            ? value.Substring(0, valueLength)
            : value;
    }

    // NB! we can't pass `ReadOnlySpan<char>` as state (as it's ref struct), see: https://github.com/dotnet/runtime/issues/30175
    private static string SanitizeToNewString(string source, int length, int firstUnsafeCharPosition) =>
        string.Create(length, (firstUnsafeCharPosition, source), CreateValue);

    private static void CreateValue(Span<char> buffer, (int FirstUnsafeCharPos, string SourceValue) state)
    {
        (int firstUnsafeCharPosition, string source) = state;

        source.AsSpan(0, firstUnsafeCharPosition).CopyTo(buffer);
        for (int i = firstUnsafeCharPosition; i < buffer.Length; i++)
        {
            var c = source[i];
            buffer[i] = IsUnsafeChar(c)
                ? CorrelationIdValueSanitizer.SanitizedChar
                : c;
        }
    }

    private static bool IsUnsafeChar(char c) => !SafeCorrelationIdChars.Contains(c);
}
#endif