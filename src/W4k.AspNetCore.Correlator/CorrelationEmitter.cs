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
            string? responseHeaderName = GetResponseHeaderName(correlationContext);
            if (responseHeaderName != null)
            {
                httpContext.Response.Headers.AddHeaderIfNotSet(
                    responseHeaderName,
                    correlationContext.CorrelationId);
            }

            return Task.CompletedTask;
        }

        private string? GetResponseHeaderName(CorrelationContext correlationContext)
        {
            if (_options.Emit.Settings == HeaderPropagation.UsePredefinedHeaderName)
            {
                return _options.Emit.HeaderName;
            }

            if (_options.Emit.Settings == HeaderPropagation.KeepIncomingHeaderName
                && correlationContext is RequestCorrelationContext requestCorrelationContext)
            {
                return requestCorrelationContext.Header;
            }

            return null;
        }
    }
}
