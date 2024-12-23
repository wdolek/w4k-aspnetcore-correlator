using System;

namespace W4k.AspNetCore.Correlator;

/// <summary>
/// Request correlation ID.
/// </summary>
public sealed class CorrelationId : IEquatable<CorrelationId>
{
    /// <summary>
    /// Empty correlation ID.
    /// </summary>
    public static readonly CorrelationId Empty = new(string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationId"/> class.
    /// </summary>
    /// <param name="correlationId">Correlation ID value.</param>
    private CorrelationId(string correlationId)
    {
        Value = correlationId;
    }

    /// <summary>
    /// Gets correlation ID value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether correlation ID is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <summary>
    /// Implicit type cast to <see cref="string"/>.
    /// </summary>
    /// <param name="correlationId">Correlation ID to be cast to string.</param>
    public static implicit operator string(CorrelationId correlationId) =>
        correlationId?.Value ?? string.Empty;

    /// <summary>
    /// Performs equal comparison between two values.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    /// <returns>
    /// Returns <c>true</c> if both values are equal, <c>false</c> otherwise.
    /// </returns>
    public static bool operator ==(CorrelationId? left, CorrelationId? right) =>
        left is null
            ? right is null
            : left.Equals(right);

    /// <summary>
    /// Performs equal comparison between two values.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    /// <returns>
    /// Returns <c>true</c> if values differ, <c>false</c> otherwise.
    /// </returns>
    public static bool operator !=(CorrelationId? left, CorrelationId? right) => !(left == right);

    /// <summary>
    /// Creates new correlation ID.
    /// </summary>
    /// <param name="value">Correlation ID value.</param>
    /// <returns>
    /// Returns new instance of <see cref="CorrelationId"/>, <see cref="Empty"/> if <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public static CorrelationId FromString(string? value) =>
        value is null
            ? Empty
            : new CorrelationId(value);

    /// <inheritdoc />
    public override int GetHashCode() =>
        Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CorrelationId correlationId && Equals(correlationId);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public bool Equals(CorrelationId? other) =>
        string.Equals(Value, other?.Value, StringComparison.OrdinalIgnoreCase);
}