namespace W4k.AspNetCore.Correlator.Context
{
    /// <summary>
    /// Default implementation of correlation context container.
    /// </summary>
    internal sealed partial class CorrelationContextContainer
    {
        /// <summary>
        /// Disposable correlation scope.
        /// </summary>
        internal sealed class CorrelationScope : ICorrelationScope
        {
            private readonly CorrelationContextContainer _container;

            /// <summary>
            /// Initializes a new instance of the <see cref="CorrelationScope"/> class.
            /// </summary>
            /// <param name="container">Related correlation context container.</param>
            public CorrelationScope(CorrelationContextContainer container)
            {
                _container = container;
            }

            /// <inheritdoc/>
            public CorrelationContext CorrelationContext => _container.CorrelationContext;

            /// <inheritdoc/>
            public void Dispose() => _container.Dispose();
        }
    }
}
