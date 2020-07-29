namespace W4k.AspNetCore.Correlator.Context
{
    public interface ICorrelationContextAccessor
    {
        CorrelationContext CorrelationContext { get; }
    }
}
