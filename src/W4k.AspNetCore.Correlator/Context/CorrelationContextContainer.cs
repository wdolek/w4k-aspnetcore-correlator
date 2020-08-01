using System;
using System.Threading;
using W4k.AspNetCore.Correlator.Context.Types;

namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Default implementation of correlation container.
    /// </summary>
    internal class CorrelationContextContainer : ICorrelationContextContainer, ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext?> LocalContext = new AsyncLocal<CorrelationContext?>();

        /// <inheritdoc/>
        public CorrelationContext CorrelationContext => LocalContext.Value ?? EmptyCorrelationContext.Instance;

        /// <inheritdoc/>
        public IDisposable CreateScope(CorrelationContext correlationContext)
        {
            LocalContext.Value = correlationContext;

            return new CorrelationScope();
        }

        /// <summary>
        /// Disposable correlation scope.
        /// </summary>
        internal sealed class CorrelationScope : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose()
            {
                LocalContext.Value = null;
            }
        }
    }
}
