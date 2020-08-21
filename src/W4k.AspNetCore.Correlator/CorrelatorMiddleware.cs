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
            ILogger logger)
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

                RegisterEmitter(_options.Emit, httpContext);
                ReplaceTraceIdentifier(httpContext, correlationId);

                using (BeginCorrelatedLoggingScope(_options.LoggingScope, correlationId))
                {
                    await _next.Invoke(httpContext);
                }
            }
        }

        /// <summary>
        /// Registers correlation emitter to <see cref="HttpResponse.OnStarting(Func{object, Task}, object)"/>.
        /// </summary>
        /// <param name="propagationSettings">Header propagation settings.</param>
        /// <param name="httpContext">Current HTTP context.</param>
        private void RegisterEmitter(PropagationSettings propagationSettings, HttpContext httpContext)
        {
            if (propagationSettings.Settings == HeaderPropagation.NoPropagation)
            {
                return;
            }

            httpContext.Response.OnStarting(ctx => _emitter.Emit((HttpContext)ctx), httpContext);
        }

        /// <summary>
        /// Replaces <see cref="HttpContext.TraceIdentifier"/> by <see cref="CorrelationId"/> if not empty.
        /// </summary>
        /// <param name="httpContext">Current HTTP context.</param>
        /// <param name="correlationId">Current correlation ID.</param>
        /// <returns>
        /// HTTP context where trace identifier is replaced by correlation ID.
        /// </returns>
        private HttpContext ReplaceTraceIdentifier(HttpContext httpContext, CorrelationId correlationId)
        {
            if (_options.ReplaceTraceIdentifier || correlationId.IsEmpty)
            {
                return httpContext;
            }

            httpContext.TraceIdentifier = correlationId;

            return httpContext;
        }

        /// <summary>
        /// Begins logging scope containing correlation ID.
        /// </summary>
        /// <param name="scopeSettings">Logging scope settings.</param>
        /// <param name="correlationId">Correlation ID.</param>
        /// <returns>
        /// Returns new logger scope or <c>null</c> if correlation ID should not be included in scope or when is empty.
        /// </returns>
        private IDisposable? BeginCorrelatedLoggingScope(LoggingScopeSettings scopeSettings, CorrelationId correlationId)
        {
            if (scopeSettings.IncludeScope)
            {
                return null;
            }

            if (correlationId.IsEmpty)
            {
                return null;
            }

            return _logger.BeginScope(
                new Dictionary<string, object>
                {
                    [scopeSettings.CorrelationKey] = correlationId,
                });
        }
    }
}
