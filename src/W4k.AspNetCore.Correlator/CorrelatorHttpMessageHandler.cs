using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Delegating HTTP message handler handling correlation ID in outgoing request.
    /// </summary>
    public class CorrelatorHttpMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly CorrelatorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="contextAccessor">HTTP context accessor.</param>
        /// <param name="options">Correlator options.</param>
        public CorrelatorHttpMessageHandler(IHttpContextAccessor contextAccessor, IOptions<CorrelatorOptions> options)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpContext context = _contextAccessor.HttpContext;
            if (context != null)
            {
                void AddCorrelationId(string headerName) =>
                    request.Headers.AddIfNotSet(headerName, context.TraceIdentifier);

                _options.Forward
                    .OnPredefinedHeader(s => AddCorrelationId(s.HeaderName))
                    .OnIncomingHeader(
                        s => AddCorrelationId(context.Request.Headers.GetCorrelationHeaderName(_options.ReadFrom)));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
