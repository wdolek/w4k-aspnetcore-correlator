using System;
using System.Buffers;
using W4k.AspNetCore.Correlator.Logging;

namespace W4k.AspNetCore.Correlator.Validation;

/// <summary>
/// Correlation value pattern validator: value is valid when it is non-empty, does not exceed
/// maximum length and consists of characters considered safe for logging only.
/// </summary>
/// <remarks>
/// <para>
/// The allowlist of characters matches the set used for log value sanitization (letters, digits
/// and <c>#</c>, <c>+</c>, <c>-</c>, <c>.</c>, <c>/</c>, <c>:</c>, <c>=</c>, <c>_</c>, <c>|</c>,
/// <c>~</c>), so a value accepted by this validator is also written to logs without modification.
/// </para>
/// <para>
/// Empty value is considered invalid.
/// </para>
/// </remarks>
public sealed class CorrelationValuePatternValidator : ICorrelationValidator
{
    /// <summary>
    /// Default maximum length of correlation ID.
    /// </summary>
    public const int DefaultMaxLength = 80;

    private static readonly SearchValues<char> SafeCorrelationIdChars =
        SearchValues.Create(CorrelationIdValueSanitizer.SafeCorrelationIdCharsString);

    private readonly int _maxLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationValuePatternValidator"/> class.
    /// </summary>
    /// <param name="maxLength">Maximum length of correlation ID, <see cref="DefaultMaxLength"/> when not provided.</param>
    public CorrelationValuePatternValidator(int maxLength = DefaultMaxLength)
    {
        _maxLength = maxLength;
    }

    /// <inheritdoc/>
    public ValidationResult Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ValidationResult.Invalid("Value is null or empty");
        }

        if (value.Length > _maxLength)
        {
            return ValidationResult.Invalid($"Received value of length: {value.Length}, expecting max length {_maxLength}");
        }

        return value.AsSpan().IndexOfAnyExcept(SafeCorrelationIdChars) >= 0
            ? ValidationResult.Invalid("Value contains characters outside of the allowed set")
            : ValidationResult.Valid;
    }
}