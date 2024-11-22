using System;

namespace W4k.AspNetCore.Correlator.Validation;

/// <summary>
/// Correlation validation result.
/// </summary>
public readonly struct ValidationResult : IEquatable<ValidationResult>
{
    /// <summary>
    /// Valid result, correlation ID is valid.
    /// </summary>
    public static readonly ValidationResult Valid = new(true, string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> struct.
    /// </summary>
    private ValidationResult(bool isValid, string reason)
    {
        IsValid = isValid;
        Reason = reason;
    }

    /// <summary>
    /// Gets a value indicating whether value is valid (<c>true</c>).
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets reason message when value is invalid.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Performs equal comparison between two values.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    /// <returns>
    /// Returns <c>true</c> if both values are equal, <c>false</c> otherwise.
    /// </returns>
    public static bool operator ==(ValidationResult left, ValidationResult right) =>
        left.Equals(right);

    /// <summary>
    /// Performs equal comparison between two values.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    /// <returns>
    /// Returns <c>true</c> if values differ, <c>false</c> otherwise.
    /// </returns>
    public static bool operator !=(ValidationResult left, ValidationResult right) => !(left == right);

    /// <summary>
    /// Creates invalid validation result.
    /// </summary>
    /// <param name="reason">Reason for value being invalid.</param>
    /// <returns>
    /// Invalid validation result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reason"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reason"/> is empty.</exception>
    public static ValidationResult Invalid(string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(reason);
        return new ValidationResult(false, reason);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsValid);
        hashCode.Add(Reason, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is ValidationResult validationResult && Equals(validationResult);

    /// <inheritdoc />
    public bool Equals(ValidationResult other) =>
        IsValid == other.IsValid && string.Equals(Reason, other.Reason, StringComparison.Ordinal);
}