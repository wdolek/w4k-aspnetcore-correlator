using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http.Extensions;
using W4k.AspNetCore.Correlator.Logging;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Http
{
    internal class CorrelationEmitter : ICorrelationEmitter
    {
        private readonly CorrelatorOptions _options;
        private readonly ILogger<CorrelationEmitter> _logger;

        public CorrelationEmitter(IOptions<CorrelatorOptions> options, ILogger<CorrelationEmitter> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Task Emit(HttpContext httpContext, CorrelationContext correlationContext)
        {
            var responseHeaderName = GetResponseHeaderName(_options.Emit, correlationContext);
            if (responseHeaderName is null)
            {
                return Task.CompletedTask;
            }

            _logger.WritingCorrelatedResponse();

            httpContext.Response.Headers.AddHeaderIfNotSet(
                responseHeaderName,
                correlationContext.CorrelationId);

            return Task.CompletedTask;
        }

        private static string? GetResponseHeaderName(
            PropagationSettings propagation,
            CorrelationContext correlationContext) =>
            (propagation.Settings, correlationContext) switch
            {
                // use predefined header name
                (HeaderPropagation.UsePredefinedHeaderName, _) => propagation.HeaderName,

                // keep incoming header name, correlation ID received
                (HeaderPropagation.KeepIncomingHeaderName, RequestCorrelationContext requestCorrelationContext) =>
                    requestCorrelationContext.Header,

                // keep incoming header name, correlation ID generated
                (HeaderPropagation.KeepIncomingHeaderName, GeneratedCorrelationContext _) => propagation.HeaderName,

                // no propagation, not received and not generated, received invalid value
                _ => null,
            };
    }
}
