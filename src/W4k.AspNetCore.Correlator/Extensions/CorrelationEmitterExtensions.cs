using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="ICorrelationEmitter"/>.
    /// </summary>
    internal static class CorrelationEmitterExtensions
    {
        /// <summary>
        /// Hooks correlation emitter to HTTP context response.
        /// </summary>
        /// <param name="emitter">Correlation emitter.</param>
        /// <param name="httpContext">HTTP context.</param>
        public static void Register(this ICorrelationEmitter emitter, HttpContext httpContext) =>
            httpContext.Response.OnStarting(ctx => emitter.Emit((HttpContext)ctx), httpContext);
    }
}
