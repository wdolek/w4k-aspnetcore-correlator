using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Http
{
    /// <summary>
    /// Delegating HTTP message handler handling correlation ID in outgoing request.
    /// </summary>
    public sealed class CorrelatorHttpMessageHandler : DelegatingHandler
    {
        private readonly PropagationSettings _settings;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="settings">Propagation settings.</param>
        /// <param name="correlationContextAccessor">Correlation context accessor.</param>
        public CorrelatorHttpMessageHandler(
            PropagationSettings settings,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            ThrowHelper.ThrowIfNull(correlationContextAccessor, nameof(correlationContextAccessor));
            _settings = settings;
            _correlationContextAccessor = correlationContextAccessor;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ThrowHelper.ThrowIfNull(request, nameof(request));
            HandleCorrelationIdForwarding(request.Headers);

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Handles correlation ID forwarding.
        /// </summary>
        /// <param name="requestHeaders">HTTP request headers.</param>
        /// <returns>
        /// HTTP request headers containing correlation ID (or unchanged, if forwarding not set).
        /// </returns>
        private HttpRequestHeaders HandleCorrelationIdForwarding(HttpRequestHeaders requestHeaders)
        {
            if (_settings.Settings == HeaderPropagation.NoPropagation)
            {
                return requestHeaders;
            }

            var correlationContext = _correlationContextAccessor.CorrelationContext;

            return (_settings.Settings, correlationContext) switch
            {
                (HeaderPropagation.UsePredefinedHeaderName, _) =>
                    requestHeaders.AddHeaderIfNotSet(
                        _settings.HeaderName,
                        correlationContext.CorrelationId),

                (HeaderPropagation.KeepIncomingHeaderName, RequestCorrelationContext requestCorrelationContext) =>
                    requestHeaders.AddHeaderIfNotSet(
                        requestCorrelationContext.Header,
                        requestCorrelationContext.CorrelationId),

                (HeaderPropagation.KeepIncomingHeaderName, GeneratedCorrelationContext generatedCorrelationContext) =>
                    requestHeaders.AddHeaderIfNotSet(
                        _settings.HeaderName,
                        generatedCorrelationContext.CorrelationId),

                _ => requestHeaders,
            };
        }
    }
}
