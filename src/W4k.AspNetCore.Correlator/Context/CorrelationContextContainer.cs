using System;
using System.Threading;

namespace W4k.AspNetCore.Correlator.Context
{
    internal class CorrelationContextContainer : ICorrelationContextContainer, ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext?> LocalContext = new AsyncLocal<CorrelationContext?>();

        public CorrelationContext? CorrelationContext => LocalContext.Value;

        public IDisposable CreateScope(CorrelationContext correlationContext)
        {
            LocalContext.Value = correlationContext;

            return new CorrelationScope();
        }

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
