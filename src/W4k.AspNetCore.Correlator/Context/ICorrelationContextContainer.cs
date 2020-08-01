using System;

namespace W4k.AspNetCore.Correlator.Context
{
    internal interface ICorrelationContextContainer
    {
        IDisposable CreateScope(CorrelationContext correlationContext);
    }
}
