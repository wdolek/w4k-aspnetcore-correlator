using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Extensions;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Delegating HTTP message handler handling correlation ID in outgoing request.
    /// </summary>
    public class CorrelatorHttpMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="contextAccessor">HTTP context accessor.</param>
        public CorrelatorHttpMessageHandler(IHttpContextAccessor contextAccessor)
            : base()
        {
            _contextAccessor = contextAccessor ?? throw new System.ArgumentNullException(nameof(contextAccessor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">Inner HTTP message handler.</param>
        /// <param name="contextAccessor">HTTP context accessor.</param>
        public CorrelatorHttpMessageHandler(HttpMessageHandler innerHandler, IHttpContextAccessor contextAccessor)
            : base(innerHandler)
        {
            _contextAccessor = contextAccessor ?? throw new System.ArgumentNullException(nameof(contextAccessor));
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // TODO: Set request header according to settings
            request.AddHeaderIfNotSet(HttpHeaders.CorrelationId, _contextAccessor.HttpContext?.TraceIdentifier);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
