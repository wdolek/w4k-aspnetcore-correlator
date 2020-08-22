using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
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
        private readonly ICorrelationEmitter _emitter;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">Delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">Correlator options.</param>
        /// <param name="correlationContextFactory">Correlation context factory.</param>
        /// <param name="correlationContextContainer">Correlation context container.</param>
        /// <param name="correlationEmitter">Correlation emitter.</param>
        /// <param name="logger">Logger.</param>
        public CorrelatorMiddleware(
            RequestDelegate next,
            IOptions<CorrelatorOptions> options,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationContextContainer correlationContextContainer,
            ICorrelationEmitter correlationEmitter,
            ILogger<CorrelatorMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _contextFactory = correlationContextFactory;
            _contextContainer = correlationContextContainer;
            _emitter = correlationEmitter;
            _logger = logger;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>
        /// A task that represents the execution of this middleware.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            var correlationContext = _contextFactory.CreateContext(httpContext);
            using (_contextContainer.CreateScope(correlationContext))
            {
                var correlationId = correlationContext.CorrelationId;

                // emit correlation ID back to caller in response headers
                if (_options.Emit.Settings != HeaderPropagation.NoPropagation)
                {
                    httpContext.Response.OnStarting(() => _emitter.Emit(httpContext, correlationContext));
                }

                // assign correlation ID to ASP.NET `TraceIdentifier` property
                // (causes correlation ID to appear in trace logs instead generated trace ID)
                if (_options.ReplaceTraceIdentifier)
                {
                    ReplaceTraceIdentifier(httpContext, correlationId);
                }

                // create logging scope or await next middleware right away
                // (state is shared via scope provider with other logger instances)
                if (_options.LoggingScope.IncludeScope)
                {
                    using (BeginCorrelatedLoggingScope(_options.LoggingScope.CorrelationKey, correlationId))
                    {
                        await _next.Invoke(httpContext);
                    }
                }
                else
                {
                    await _next.Invoke(httpContext);
                }
            }
        }

        /// <summary>
        /// Replaces <see cref="HttpContext.TraceIdentifier"/> by <see cref="CorrelationId"/> if not empty.
        /// </summary>
        /// <param name="httpContext">Current HTTP context.</param>
        /// <param name="correlationId">Current correlation ID.</param>
        private static void ReplaceTraceIdentifier(HttpContext httpContext, CorrelationId correlationId)
        {
            if (correlationId.IsEmpty)
            {
                return;
            }

            httpContext.TraceIdentifier = correlationId;
        }

        /// <summary>
        /// Begins logging scope containing correlation ID.
        /// </summary>
        /// <param name="correlationKey">Logging scope correlation key.</param>
        /// <param name="correlationId">Correlation ID.</param>
        /// <returns>
        /// Returns new logger scope or <c>null</c> if correlation ID should not be included in scope or when is empty.
        /// </returns>
        private IDisposable? BeginCorrelatedLoggingScope(string correlationKey, CorrelationId correlationId)
        {
            if (correlationId.IsEmpty)
            {
                return null;
            }

            return _logger.BeginScope(
                new Dictionary<string, object>
                {
                    [correlationKey] = correlationId,
                });
        }
    }
}
