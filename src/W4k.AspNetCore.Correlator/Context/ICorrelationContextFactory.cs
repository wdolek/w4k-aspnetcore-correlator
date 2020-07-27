using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Context
{
    public interface ICorrelationContextFactory
    {
        CorrelationContext CreateContext(HttpContext httpContext);
    }
}
