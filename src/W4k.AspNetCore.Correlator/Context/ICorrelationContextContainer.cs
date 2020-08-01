using System;

namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Correlation context container.
    /// </summary>
    internal interface ICorrelationContextContainer
    {
        /// <summary>
        /// Creates disposable correlation scope using given context.
        /// </summary>
        /// <param name="correlationContext">Correlation context.</param>
        /// <returns>
        /// Disposable scope.
        /// </returns>
        IDisposable CreateScope(CorrelationContext correlationContext);
    }
}
