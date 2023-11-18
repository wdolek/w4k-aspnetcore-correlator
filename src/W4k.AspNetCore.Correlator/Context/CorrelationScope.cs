namespace W4k.AspNetCore.Correlator.Context;

internal class CorrelationScope : ICorrelationScope
{
    private readonly CorrelationContextContainer _container;

    public CorrelationScope(CorrelationContextContainer container)
    {
        _container = container;
    }

    public CorrelationContext CorrelationContext => _container.CorrelationContext;

    public void Dispose() => _container.Dispose();
}