using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Context.Types;

namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Default implementation of correlation context container.
    /// </summary>
    internal sealed partial class CorrelationContextContainer
        : ICorrelationScopeFactory, ICorrelationContextAccessor, IDisposable
    {
        private static readonly AsyncLocal<CorrelationContext?> LocalContext = new AsyncLocal<CorrelationContext?>();

        private readonly ICorrelationContextFactory _correlationContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContextContainer"/> class.
        /// </summary>
        /// <param name="correlationContextFactory">Correlation context factory.</param>
        public CorrelationContextContainer(ICorrelationContextFactory correlationContextFactory)
        {
            _correlationContextFactory = correlationContextFactory;
        }

        /// <inheritdoc/>
        public CorrelationContext CorrelationContext => LocalContext.Value ?? EmptyCorrelationContext.Instance;

        /// <inheritdoc/>
        public ICorrelationScope CreateScope(HttpContext httpContext)
        {
            LocalContext.Value = _correlationContextFactory.CreateContext(httpContext);

            return new CorrelationScope(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            LocalContext.Value = null;
        }
    }
}
