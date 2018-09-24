using System;

namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Header propagation settings.
    /// </summary>
    public struct PropagationSettings : IEquatable<PropagationSettings>
    {
        /// <summary>
        /// Don't propagate header.
        /// </summary>
        public static readonly PropagationSettings NoPropagation =
            new PropagationSettings(HeaderPropagation.NoPropagation, string.Empty);

        /// <summary>
        /// Propagate header with original name.
        /// </summary>
        public static readonly PropagationSettings KeepIncomingHeaderName =
            new PropagationSettings(HeaderPropagation.KeepIncomingHeaderName, string.Empty);

        /// <summary>
        /// Internal header name.
        /// </summary>
        private readonly string _headerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagationSettings"/> struct.
        /// </summary>
        /// <param name="propagation">Header propagation.</param>
        /// <param name="headerName">Header name.</param>
        private PropagationSettings(HeaderPropagation propagation, string headerName)
        {
            Settings = propagation;
            _headerName = headerName;
        }

        /// <summary>
        /// Gets header propagation.
        /// </summary>
        public HeaderPropagation Settings { get; }

        /// <summary>
        /// Gets custom header name (not set for all header propagation types).
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="HttpHeaders.CorrelationId"/> if struct instantiated using <c>default</c>.
        /// </remarks>
        public string HeaderName => _headerName ?? HttpHeaders.CorrelationId;

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if both values are equal, <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(PropagationSettings left, PropagationSettings right) =>
            left.Equals(right);

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if values differ, <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(PropagationSettings left, PropagationSettings right) =>
            !left.Equals(right);

        /// <summary>
        /// Creates settings to propagate header with provided name.
        /// </summary>
        /// <param name="headerName">Custom header name.</param>
        /// <returns>
        /// Propagation settings with custom header name.
        /// </returns>
        public static PropagationSettings PropagateAs(string headerName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                throw new ArgumentNullException(nameof(headerName));
            }

            return new PropagationSettings(HeaderPropagation.UsePredefinedHeaderName, headerName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is PropagationSettings other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * Settings.GetHashCode() ^ HeaderName.GetHashCode();
            }
        }

        /// <inheritdoc />
        public bool Equals(PropagationSettings other) =>
            Settings == other.Settings
            && string.Equals(HeaderName, other.HeaderName, StringComparison.OrdinalIgnoreCase);
    }
}
