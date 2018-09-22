using System;
using System.Globalization;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlation ID structure.
    /// </summary>
    public struct CorrelationId : IEquatable<CorrelationId>
    {
        /// <summary>
        /// Empty correlation ID.
        /// </summary>
        public static readonly CorrelationId Empty = new CorrelationId(string.Empty);

        /// <summary>
        /// Internal correlation ID value.
        /// </summary>
        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationId"/> struct.
        /// </summary>
        /// <param name="correlationId">Correlation ID value.</param>
        private CorrelationId(string correlationId)
        {
            _value = correlationId;
        }

        /// <summary>
        /// Gets correlation ID value.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Empty"/> if internal value is <c>null</c> (may happen when instantiated via <c>default</c>).
        /// </remarks>
        public string Value => _value ?? Empty;

        /// <summary>
        /// Implicit type cast to <see cref="string"/>.
        /// </summary>
        /// <param name="correlationId">Correlation ID to be cast to string.</param>
        public static implicit operator string(CorrelationId correlationId) => correlationId.Value;

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if both values are equal, <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(CorrelationId left, CorrelationId right) => left.Equals(right);

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if values differ, <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(CorrelationId left, CorrelationId right) => !left.Equals(right);

        /// <summary>
        /// Creates new correlation ID.
        /// </summary>
        /// <param name="value">Correlation ID value.</param>
        /// <returns>
        /// Returns new instance of <see cref="CorrelationId"/> or <c>null</c> if value is invalid.
        /// </returns>
        public static CorrelationId? FromString(string value) =>
            !string.IsNullOrEmpty(value)
                ? new CorrelationId(value)
                : (CorrelationId?)null;

        /// <summary>
        /// Generates new correlation ID.
        /// </summary>
        /// <returns>
        /// Returns new instance of correlation ID.
        /// </returns>
        public static CorrelationId NewCorrelationId() =>
            new CorrelationId(Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture));

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is CorrelationId correlationId
                ? Equals(correlationId)
                : false;

        /// <inheritdoc />
        public override string ToString() => _value;

        /// <summary>
        /// Indicates whether the current correlation ID is equal to another one.
        /// </summary>
        /// <param name="other">Correlation ID to be compared to.</param>
        /// <returns>
        /// Returns <c>true</c> if given correlation ID equals, <c>false</c> otherwise.
        /// </returns>
        public bool Equals(CorrelationId other) =>
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }
}
