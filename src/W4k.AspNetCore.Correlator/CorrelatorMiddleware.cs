using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlator middleware for reading correlation ID from incoming HTTP request.
    /// </summary>
    internal class CorrelatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICorrelationContextFactory _contextFactory;
        private readonly ICorrelationContextContainer _contextContainer;
        private readonly ICorrelationEmitter _emitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">Delegate representing the next middleware in the request pipeline.</param>
        /// <param name="correlationContextFactory">Correlation context factory.</param>
        /// <param name="correlationContextContainer">Correlation context container.</param>
        /// <param name="correlationEmitter">Correlation emitter.</param>
        public CorrelatorMiddleware(
            RequestDelegate next,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationContextContainer correlationContextContainer,
            ICorrelationEmitter correlationEmitter)
        {
            _next = next;
            _contextFactory = correlationContextFactory;
            _contextContainer = correlationContextContainer;
            _emitter = correlationEmitter;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>
        /// A task that represents the execution of this middleware.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062", Justification = "If HTTP context is null, we have bigger problem here.")]
        public async Task Invoke(HttpContext httpContext)
        {
            var correlationContext = _contextFactory.CreateContext(httpContext);
            using (_contextContainer.CreateScope(correlationContext))
            {
                httpContext.Response.OnStarting(ctx => _emitter.Emit((HttpContext)ctx), httpContext);

                await _next.Invoke(httpContext.WithCorrelationId(correlationContext.CorrelationId));
            }
        }
    }
}
