using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Http;

namespace W4k.AspNetCore.Correlator.Options
{
    /// <summary>
    /// Correlator options.
    /// </summary>
    public sealed class CorrelatorOptions
    {
        /// <summary>
        /// Gets collection of header names to be used when reading correlation ID from request headers.
        /// </summary>
        public ICollection<string> ReadFrom { get; } =
            new List<string>
            {
                HttpHeaders.CorrelationId,
                HttpHeaders.RequestId,
                HttpHeaders.AspNetRequestId,
            };

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

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="HttpContext.TraceIdentifier"/> is replaced by correlation ID or left intact.
        /// </summary>
        public bool ReplaceTraceIdentifier { get; set; } = false;

        /// <summary>
        /// Gets or sets logging scope settings.
        /// </summary>
        public LoggingScopeSettings LoggingScope { get; set; } = LoggingScopeSettings.NoScope;
    }
}
