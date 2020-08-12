using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Delegating HTTP message handler handling correlation ID in outgoing request.
    /// </summary>
    public sealed class CorrelatorHttpMessageHandler : DelegatingHandler
    {
        private readonly CorrelatorOptions _options;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="options">Correlator options.</param>
        /// <param name="correlationContextAccessor">Correlation context accessor.</param>
        public CorrelatorHttpMessageHandler(
            IOptions<CorrelatorOptions> options,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor
                ?? throw new ArgumentNullException(nameof(correlationContextAccessor));

            _ = options ?? throw new ArgumentNullException(nameof(options));
            _options = options.Value;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

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
            var propagation = _options.Forward;
            if (propagation.Settings == HeaderPropagation.NoPropagation)
            {
                return requestHeaders;
            }

            var correlationContext = _correlationContextAccessor.CorrelationContext;

            return (propagation.Settings, correlationContext) switch
            {
                (HeaderPropagation.UsePredefinedHeaderName, _) =>
                    requestHeaders.AddHeaderIfNotSet(
                        propagation.HeaderName,
                        correlationContext.CorrelationId),

                (HeaderPropagation.KeepIncomingHeaderName, RequestCorrelationContext requestCorrelationContext) =>
                    requestHeaders.AddHeaderIfNotSet(
                        requestCorrelationContext.Header,
                        requestCorrelationContext.CorrelationId),

                (HeaderPropagation.KeepIncomingHeaderName, GeneratedCorrelationContext generatedCorrelationContext) =>
                    requestHeaders.AddHeaderIfNotSet(
                        propagation.HeaderName,
                        generatedCorrelationContext.CorrelationId),

                _ => requestHeaders,
            };
        }
    }
}
