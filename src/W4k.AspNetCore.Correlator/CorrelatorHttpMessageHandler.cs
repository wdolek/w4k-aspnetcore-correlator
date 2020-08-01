using System;
using System.Net.Http;
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
    public class CorrelatorHttpMessageHandler : DelegatingHandler
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
            var correlationContext = _correlationContextAccessor.CorrelationContext;
            var correlationId = correlationContext.CorrelationId;

            if (!correlationId.IsEmpty)
            {
                _options.Forward
                    .OnPredefinedHeader(s => request.Headers.AddHeaderIfNotSet(s.HeaderName, correlationId))
                    .OnIncomingHeader(_ =>
                    {
                        var requestCorrelationContext = correlationContext as RequestCorrelationContext;
                        request.Headers.AddHeaderIfNotSet(requestCorrelationContext?.Header, correlationId);
                    });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
