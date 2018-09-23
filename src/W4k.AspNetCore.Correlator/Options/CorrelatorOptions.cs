using System;
using System.Collections.Generic;

namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Correlator (middleware) options.
    /// </summary>
    public class CorrelatorOptions
    {
        /// <summary>
        /// Gets list of header names to be used when reading correlation ID from request headers.
        /// </summary>
        /// <remarks>
        /// Order of entries matters! First header with non-empty value is used.
        /// </remarks>
        public List<string> ReadFrom { get; } = new List<string>
            {
                HttpHeaders.CorrelationId,
                HttpHeaders.RequestId,
            };

        /// <summary>
        /// Gets or sets factory of correlation IDs. If <c>null</c>, correlation ID is not generated.
        /// </summary>
        public Func<CorrelationId> Factory { get; set; } = () => CorrelationId.NewCorrelationId();

        /// <summary>
        /// Gets or sets correlation ID propagation settings affecting response headers.
        /// </summary>
        public PropagationSettings Emit { get; set; } = PropagationSettings.PropagateAs(HttpHeaders.CorrelationId);

        /// <summary>
        /// Gets or sets correlation ID propagation settings affecting subsequent requests via <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        public PropagationSettings Forward { get; set; } = PropagationSettings.PropagateAs(HttpHeaders.CorrelationId);
    }
}
