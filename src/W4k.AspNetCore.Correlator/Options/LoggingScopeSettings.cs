using System;

namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Logging scope settings, <see cref="Microsoft.Extensions.Logging.ILogger.BeginScope{TState}(TState)"/>.
    /// </summary>
    public readonly struct LoggingScopeSettings : IEquatable<LoggingScopeSettings>
    {
        /// <summary>
        /// Configure no scope - correlation ID won't be added to logger scope.
        /// </summary>
        public static readonly LoggingScopeSettings NoScope = new LoggingScopeSettings(false, string.Empty);

        /// <summary>
        /// Default logging scope.
        /// </summary>
        private const string DefaultLoggingScope = "Correlation";

        /// <summary>
        /// Internal correlation scope key.
        /// </summary>
        private readonly string _correlationKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingScopeSettings"/> struct.
        /// </summary>
        /// <param name="includeScope">Indicates whether correlation ID should be added to logger scope.</param>
        /// <param name="key">Correlation ID logger scope key.</param>
        private LoggingScopeSettings(bool includeScope, string key)
        {
            IncludeScope = includeScope;
            _correlationKey = key;
        }

        /// <summary>
        /// Gets a value indicating whether correlation ID should be added to logger scope.
        /// </summary>
        public bool IncludeScope { get; }

        /// <summary>
        /// Gets logger scope key of correlation ID.
        /// </summary>
        public string CorrelationKey => _correlationKey ?? DefaultLoggingScope;

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if both values are equal, <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(LoggingScopeSettings left, LoggingScopeSettings right) =>
            left.Equals(right);

        /// <summary>
        /// Performs equal comparison between two values.
        /// </summary>
        /// <param name="left">Left value.</param>
        /// <param name="right">Right value.</param>
        /// <returns>
        /// Returns <c>true</c> if values differ, <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(LoggingScopeSettings left, LoggingScopeSettings right) =>
            !left.Equals(right);

        /// <summary>
        /// Creates setting for including correlation ID within logger scope, using <paramref name="correlationKey"/> as key.
        /// </summary>
        /// <param name="correlationKey">Logger scope key.</param>
        /// <returns>
        /// New instance of logging scope settings.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="correlationKey"/> is <c>null</c> or empty string.</exception>
        public static LoggingScopeSettings IncludeLoggingScope(string correlationKey = DefaultLoggingScope)
        {
            if (string.IsNullOrEmpty(correlationKey))
            {
                throw new ArgumentNullException(nameof(correlationKey));
            }

            return new LoggingScopeSettings(true, correlationKey);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is LoggingScopeSettings other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
#if NETSTANDARD2_0
                return 17 * IncludeScope.GetHashCode() ^ CorrelationKey.GetHashCode();
#else
                return 17 * IncludeScope.GetHashCode() ^ CorrelationKey.GetHashCode(StringComparison.OrdinalIgnoreCase);
#endif
            }
        }

        /// <inheritdoc />
        public bool Equals(LoggingScopeSettings other) =>
            IncludeScope == other.IncludeScope
            && string.Equals(CorrelationKey, other.CorrelationKey, StringComparison.OrdinalIgnoreCase);
    }
}
