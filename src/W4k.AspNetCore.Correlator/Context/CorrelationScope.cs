using System;
using System.Threading;

namespace W4k.AspNetCore.Correlator.Context
{
    internal sealed class CorrelationScope : IDisposable
    {
        private readonly AsyncLocal<CorrelationContext?> _localContext;

        public CorrelationScope(AsyncLocal<CorrelationContext?> localContext)
        {
            _localContext = localContext;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _localContext.Value = null;
        }
    }
}
