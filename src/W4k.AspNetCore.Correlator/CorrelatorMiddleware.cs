﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Catch anything to prevent request to fail just for correlation")]
        public async Task Invoke(HttpContext httpContext)
        {
            ICorrelationScope? correlationScope = null;
            try
            {
                correlationScope = _scopeFactory.CreateScope(httpContext);
            }
            catch
            {
                await _next.Invoke(httpContext);
                return;
            }

            using (correlationScope)
            {
                await Invoke(httpContext, correlationScope.CorrelationContext);
            }
        }

        private async Task Invoke(HttpContext httpContext, CorrelationContext correlationContext)
        {
            // emit correlation ID back to caller in response headers
            if (_options.Emit.Settings != HeaderPropagation.NoPropagation)
            {
                httpContext.Response.OnStarting(() => EmitCorrelation(httpContext, correlationContext));
            }

            var correlationId = correlationContext.CorrelationId;

            // assign correlation ID to ASP.NET `TraceIdentifier` property
            // (causes correlation ID to appear in trace logs instead of generated trace ID)
            if (_options.ReplaceTraceIdentifier && !correlationId.IsEmpty)
            {
                httpContext.TraceIdentifier = correlationId;
            }

            // create logging scope or await next middleware right away
            // (state is shared via scope provider with other logger instances)
            if (_options.LoggingScope.IncludeScope && !correlationId.IsEmpty)
            {
                await InvokeWithLoggingScope(httpContext, _options.LoggingScope.CorrelationKey, correlationId);
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }

        private async Task InvokeWithLoggingScope(
            HttpContext httpContext,
            string correlationKey,
            CorrelationId correlationId)
        {
            var state = new Dictionary<string, object>
            {
                [correlationKey] = correlationId,
            };

            using (_logger.BeginScope(state))
            {
                await _next.Invoke(httpContext);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Catch anything to prevent request to fail when emitting correlation")]
        private Task EmitCorrelation(HttpContext httpContext, CorrelationContext correlationContext)
        {
            try
            {
                _emitter.Emit(httpContext, correlationContext);
            }
            catch
            {
                // nop
            }

            return Task.CompletedTask;
        }
    }
}
