using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    internal class CorrelationEmitter : ICorrelationEmitter
    {
        private readonly CorrelatorOptions _options;

        public CorrelationEmitter(IOptions<CorrelatorOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc/>
        public Task Emit(HttpContext httpContext, CorrelationContext correlationContext)
        {
            string? responseHeaderName = GetResponseHeaderName(_options.Emit, correlationContext);
            if (responseHeaderName != null)
            {
                httpContext.Response.Headers.AddHeaderIfNotSet(
                    responseHeaderName,
                    correlationContext.CorrelationId);
            }

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

                // no propagation or not received and not generated
                _ => null
            };
    }
}
