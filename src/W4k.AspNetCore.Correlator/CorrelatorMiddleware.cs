using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlator middleware for reading correlation ID from incoming HTTP request.
    /// </summary>
    public class CorrelatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">Delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">Correlator options.</param>
        public CorrelatorMiddleware(RequestDelegate next, IOptions<CorrelatorOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>
        /// A task that represents the execution of this middleware.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062", Justification = "If HTTP context is null, we have bigger problem here.")]
        public async Task Invoke(HttpContext httpContext)
        {
            string? requestHeaderName = httpContext.Request.Headers.GetCorrelationHeaderName(_options.ReadFrom);
            CorrelationId correlationId = httpContext.Request.Headers
                .GetCorrelationId(requestHeaderName)
                .GenerateIfEmpty(_options.Factory);

            httpContext.Response.OnStarting(
                state => EmitCorrelationId((HttpContext)state, _options.Emit, requestHeaderName, correlationId),
                httpContext);

            await _next.Invoke(httpContext.WithCorrelationId(correlationId));
        }

        /// <summary>
        /// Adds correlation ID to response headers.
        /// </summary>
        /// <param name="httpContext">HTTP context.</param>
        /// <param name="propagation">Correlation ID propagation settings.</param>
        /// <param name="incomingHeaderName">Name of header containing correlation ID on request.</param>
        /// <param name="correlationId">Correlation ID.</param>
        /// <returns>
        /// Task defining action of emitting correlation ID.
        /// </returns>
        private static Task EmitCorrelationId(
            HttpContext httpContext,
            PropagationSettings propagation,
            string? incomingHeaderName,
            CorrelationId correlationId)
        {
            string? responseHeaderName = propagation.GetCorrelationHeaderName(incomingHeaderName);
            if (!string.IsNullOrEmpty(responseHeaderName))
            {
                httpContext.Response.Headers.AddHeaderIfNotSet(responseHeaderName, correlationId);
            }

            return Task.CompletedTask;
        }
    }
}
