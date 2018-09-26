using System;
using System.Linq;
using System.Net.Http;
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
            : base()
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">Inner HTTP message handler.</param>
        /// <param name="contextAccessor">HTTP context accessor.</param>
        /// <param name="options">Correlator options.</param>
        public CorrelatorHttpMessageHandler(
            HttpMessageHandler innerHandler,
            IHttpContextAccessor contextAccessor,
            IOptions<CorrelatorOptions> options)
            : base(innerHandler)
        {
            _contextAccessor = contextAccessor ?? throw new System.ArgumentNullException(nameof(contextAccessor));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_options.Forward.Settings != HeaderPropagation.NoPropagation)
            {
                HttpContext context = _contextAccessor.HttpContext;
                if (context != null)
                {
                    var headerName = _options.Forward.Settings == HeaderPropagation.UsePredefinedHeaderName
                        ? _options.Forward.HeaderName
                        : context.Request.Headers.GetCorrelationHeaderName(_options.ReadFrom);

                    request.Headers.AddIfNotSet(headerName, context.TraceIdentifier);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
