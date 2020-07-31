namespace W4k.AspNetCore.Correlator.Context
{
    internal interface ICorrelationContextContainer
    {
        CorrelationScope CreateScope(CorrelationContext correlationContext);
    }
}
