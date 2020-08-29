using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Logging;
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
            _logger.CorrelatedRequestBegin();

            using (var correlationScope = _scopeFactory.CreateScope(httpContext))
            {
                if (ReferenceEquals(correlationScope.CorrelationContext, EmptyCorrelationContext.Instance))
                {
                    _logger.CorrelationIdMissing(httpContext.TraceIdentifier);
                }
                else
                {
                    _logger.CorrelationIdReceived(correlationScope.CorrelationContext.CorrelationId);
                }

                await Invoke(httpContext, correlationScope.CorrelationContext);
            }

            _logger.CorrelatedRequestEnd();
        }

        private async Task Invoke(HttpContext httpContext, CorrelationContext correlationContext)
        {
            // emit correlation ID back to caller in response headers
            if (_options.Emit.Settings != HeaderPropagation.NoPropagation)
            {
                _logger.WritingCorrelatedResponse();
                httpContext.Response.OnStarting(() => _emitter.Emit(httpContext, correlationContext));
            }

            var correlationId = correlationContext.CorrelationId;

            // assign correlation ID to ASP.NET `TraceIdentifier` property
            // (causes correlation ID to appear in trace logs instead of generated trace ID)
            if (_options.ReplaceTraceIdentifier && !correlationId.IsEmpty)
            {
                _logger.ReplacingTraceIdentifier(httpContext.TraceIdentifier);
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
    }
}
