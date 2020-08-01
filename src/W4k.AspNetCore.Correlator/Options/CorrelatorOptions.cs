using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Correlator (middleware) options.
    /// </summary>
    public sealed class CorrelatorOptions
    {
        /// <summary>
        /// Gets list of header names to be used when reading correlation ID from request headers.
        /// </summary>
        /// <remarks>
        /// Order of entries matters! First header with non-empty value is used.
        /// </remarks>
        public SortedSet<string> ReadFrom { get; } =
            new SortedSet<string>(
                new[]
                {
                    HttpHeaders.AspNetRequestId,
                    HttpHeaders.CorrelationId,
                    HttpHeaders.RequestId,
                },
                StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets factory of correlation IDs. If <c>null</c>, correlation ID is not generated.
        /// </summary>
        public Func<HttpContext, CorrelationId>? Factory { get; set; } =
            (_) => CorrelationId.FromString(Guid.NewGuid().ToString("D"));

        /// <summary>
        /// Gets or sets correlation ID propagation settings affecting response headers.
        /// </summary>
        public PropagationSettings Emit { get; set; } = PropagationSettings.NoPropagation;

        /// <summary>
        /// Gets or sets correlation ID propagation settings affecting subsequent requests via <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        public PropagationSettings Forward { get; set; } = PropagationSettings.PropagateAs(HttpHeaders.CorrelationId);
    }
}
