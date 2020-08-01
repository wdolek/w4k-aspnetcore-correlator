using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlator middleware for reading correlation ID from incoming HTTP request.
    /// </summary>
    internal class CorrelatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelatorOptions _options;
        private readonly ICorrelationContextFactory _contextFactory;
        private readonly ICorrelationContextContainer _contextContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">Delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">Correlator options.</param>
        /// <param name="correlationContextFactory">Correlation context factory.</param>
        /// <param name="correlationContextContainer">Correlation context container.</param>
        public CorrelatorMiddleware(
            RequestDelegate next,
            IOptions<CorrelatorOptions> options,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationContextContainer correlationContextContainer)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _contextFactory = correlationContextFactory
                ?? throw new ArgumentNullException(nameof(correlationContextFactory));

            _contextContainer = correlationContextContainer
                ?? throw new ArgumentNullException(nameof(correlationContextContainer));

            _ = options ?? throw new ArgumentNullException(nameof(options));
            _options = options.Value;
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
            var correlationContext = _contextFactory.CreateContext(httpContext);
            using (_contextContainer.CreateScope(correlationContext))
            {
                if (_options.Emit != PropagationSettings.NoPropagation)
                {
                    httpContext.Response.OnStarting(
                        state => EmitCorrelationId((HttpContext)state, correlationContext, _options),
                        httpContext);
                }

                await _next.Invoke(httpContext.WithCorrelationId(correlationContext.CorrelationId));
            }
        }

        /// <summary>
        /// Adds correlation ID to response headers.
        /// </summary>
        /// <param name="httpContext">HTTP context.</param>
        /// <param name="correlationContext">Correlation context.</param>
        /// <param name="options">Correlator options.</param>
        /// <returns>
        /// Task defining action of emitting correlation ID.
        /// </returns>
        private static Task EmitCorrelationId(
            HttpContext httpContext,
            CorrelationContext correlationContext,
            CorrelatorOptions options)
        {
            var responseHeaderName = (options.Emit.Settings, correlationContext) switch
            {
                // emit correlation ID with predefined header name
                (HeaderPropagation.UsePredefinedHeaderName, _) =>
                    options.Emit.HeaderName,

                // emit correlation ID with same header name as we received it
                (HeaderPropagation.KeepIncomingHeaderName, RequestCorrelationContext requestCorrelationContext) =>
                    requestCorrelationContext.Header,

                // emit correlation ID with first known header name when correlation has been generated
                (HeaderPropagation.KeepIncomingHeaderName, GeneratedCorrelationContext _) =>
                    options.ReadFrom.FirstOrDefault(),

                // correlation ID not received nor generated
                _ => null
            };

            if (responseHeaderName is object)
            {
                httpContext.Response.Headers.AddHeaderIfNotSet(responseHeaderName, correlationContext.CorrelationId);
            }

            return Task.CompletedTask;
        }
    }
}
