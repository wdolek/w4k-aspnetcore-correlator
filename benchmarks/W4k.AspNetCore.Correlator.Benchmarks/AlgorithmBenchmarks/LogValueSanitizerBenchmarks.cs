using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;
using Bogus;

namespace W4k.AspNetCore.Correlator.Benchmarks.AlgorithmBenchmarks;

[MemoryDiagnoser]
public class LogValueSanitizerBenchmarks
{
    private readonly string[] _correlationIds;

    [ParamsAllValues]
    public CorrelationIdLength Length { get; set; }

    [ParamsAllValues]
    public CorrelationIdContent Content { get; set; }

    public LogValueSanitizerBenchmarks()
    {
        _correlationIds = new string[1024];

        Randomizer.Seed = new Random(74656);
        var faker = new Faker();

        for (int i = 0; i < _correlationIds.Length; i++)
        {
            _correlationIds[i] = GenerateCorrelationId(faker, Length, Content);
        }
    }

    [Benchmark(Description = "Sanitize: Iterate one by one")]
    public string SanitizeIterating()
    {
        string last = null;
        foreach (var correlationId in _correlationIds)
        {
            last = CorrelationIdValueSanitizer_Iterate.Sanitize(correlationId);
        }

        return last;
    }

    [Benchmark(Description = "Sanitize: Find next, sanitize one by one")]
    public string SanitizeSearchValuesAndIterating()
    {
        string last = null;
        foreach (var correlationId in _correlationIds)
        {
            last = CorrelationIdValueSanitizer_SearchValues_IterateRestOfStr.Sanitize(correlationId);
        }

        return last;
    }

    [Benchmark(Description = "Sanitize: Find next, sanitize, repeat")]
    public string SanitizeSearchValuesAndJumpingToNextUnsafe()
    {
        string last = null;
        foreach (var correlationId in _correlationIds)
        {
            last = CorrelationIdValueSanitizer_SearchValues_JumpToNextUnsafe.Sanitize(correlationId);
        }

        return last;
    }

    private static string GenerateCorrelationId(
        Faker faker,
        CorrelationIdLength correlationIdLength,
        CorrelationIdContent correlationIdContent)
    {
        (int minLength, int maxLength) = correlationIdLength == CorrelationIdLength.Short
            ? (8, 32)
            : (100, 256);

        return correlationIdContent == CorrelationIdContent.OnlyAllowedChars
            ? faker.Random.String2(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
            : faker.Random.String2(minLength, maxLength, "ghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$&+-./:=^_|~<>\r\n\t\b@'\"{}[]?\u00a2\u00a3\u20ac\u00a5¶Æ \u00ae\u00a9‹›«»");
    }

    public enum CorrelationIdLength
    {
        Short,
        Long,
    }

    public enum CorrelationIdContent
    {
        OnlyAllowedChars,
        IncludesInvalidChars,
    }
}

file static class CorrelationIdValueSanitizer_Iterate
{
    private const int MaxValueLength = 64;
    private const char SanitizedChar = '_';

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, MaxValueLength);
        for (int i = 0; i < valueLength; i++)
        {
            if (IsUnsafeChar(value[i]))
            {
                return SanitizeToNewString(value, valueLength, i);
            }
        }

        return value.Length > MaxValueLength
            ? value.Substring(0, valueLength)
            : value;
    }

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
                ? SanitizedChar
                : c;
        }
    }

    private static bool IsUnsafeChar(char c)
    {
        if (char.IsLetterOrDigit(c))
        {
            return false;
        }

        return c < ' ' || c > 126 || c == '<' || c == '>' || c == '&' || c == '\'' || c == '\"';
    }
}

file static class CorrelationIdValueSanitizer_SearchValues_IterateRestOfStr
{
    private const int MaxValueLength = 64;
    private const char SanitizedChar = '_';

    private static readonly SearchValues<char> ValidCorrelationIdChars =
        SearchValues.Create("!#$&+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ^_`abcdefghijklmnopqrstuvwxyz|~");

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

        var firstUnsafeCharPosition = valueSpan.IndexOfAnyExcept(ValidCorrelationIdChars);
        if (firstUnsafeCharPosition >= 0)
        {
            return SanitizeToNewString(value, valueLength, firstUnsafeCharPosition);
        }

        return value.Length > MaxValueLength
            ? valueSpan.ToString()
            : value;
    }

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
                ? SanitizedChar
                : c;
        }
    }

    private static bool IsUnsafeChar(char c) =>
        !ValidCorrelationIdChars.Contains(c);
}

file static class CorrelationIdValueSanitizer_SearchValues_JumpToNextUnsafe
{
    private static readonly SearchValues<char> ValidCorrelationIdChars =
        SearchValues.Create("!#$&+-./0123456789:=ABCDEFGHIJKLMNOPQRSTUVWXYZ^_`abcdefghijklmnopqrstuvwxyz|~");

    private const int MaxValueLength = 64;
    private const char SanitizedChar = '_';

    public static string Sanitize(string value)
    {
        var valueLength = Math.Min(value.Length, MaxValueLength);
        var valueSpan = value.AsSpan(0, valueLength);

        var firstInvalidCharPos = valueSpan.IndexOfAnyExcept(ValidCorrelationIdChars);
        if (firstInvalidCharPos >= 0)
        {
            return SanitizeToNewString(value, valueLength, firstInvalidCharPos);
        }

        return value.Length > MaxValueLength
            ? valueSpan.ToString()
            : value;
    }

    private static string SanitizeToNewString(string source, int length, int firstUnsafeCharPosition) =>
        string.Create(length, (firstUnsafeCharPosition, source), CreateValue);

    private static void CreateValue(Span<char> buffer, (int FirstUnsafeCharPos, string SourceValue) state)
    {
        (int sourceIndex, string source) = state;

        // copy all safe chars before first unsafe char
        source.AsSpan(0, sourceIndex).CopyTo(buffer);

        buffer[sourceIndex] = SanitizedChar;
        ++sourceIndex;

        // jump to next unsafe char, copy all safe chars between
        while (sourceIndex < buffer.Length)
        {
            var remainingSpan = source.AsSpan(sourceIndex);
            var nextUnsafeCharPos = remainingSpan.IndexOfAnyExcept(ValidCorrelationIdChars);

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
}