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
    internal class CorrelatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelatorOptions _options;
        private readonly ICorrelationScopeFactory _scopeFactory;
        private readonly ICorrelationEmitter _emitter;
        private readonly ILogger _logger;

        public CorrelatorMiddleware(
            RequestDelegate next,
            IOptions<CorrelatorOptions> options,
            ICorrelationScopeFactory correlationScopeFactory,
            ICorrelationEmitter correlationEmitter,
            ILogger<CorrelatorMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _scopeFactory = correlationScopeFactory;
            _emitter = correlationEmitter;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            using var correlationScope = _scopeFactory.CreateScope(httpContext);
            var correlationContext = correlationScope.CorrelationContext;

            // emit correlation ID back to caller in response headers
            if (_options.Emit.Settings != HeaderPropagation.NoPropagation)
            {
                httpContext.Response.OnStarting(() => _emitter.Emit(httpContext, correlationContext));
            }

            // assign correlation ID to ASP.NET `TraceIdentifier` property
            // (causes correlation ID to appear in trace logs instead of generated trace ID)
            if (_options.ReplaceTraceIdentifier)
            {
                ReplaceTraceIdentifier(httpContext, correlationContext.CorrelationId);
            }

            // create logging scope or await next middleware right away
            // (state is shared via scope provider with other logger instances)
            if (_options.LoggingScope.IncludeScope)
            {
                using (BeginCorrelatedLoggingScope(
                    _options.LoggingScope.CorrelationKey,
                    correlationContext.CorrelationId))
                {
                    await _next.Invoke(httpContext);
                }
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }

        private static void ReplaceTraceIdentifier(HttpContext httpContext, CorrelationId correlationId)
        {
            if (correlationId.IsEmpty)
            {
                return;
            }

            httpContext.TraceIdentifier = correlationId;
        }

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
