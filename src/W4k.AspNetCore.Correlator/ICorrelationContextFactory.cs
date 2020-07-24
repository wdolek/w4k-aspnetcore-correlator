using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator
{
    public interface ICorrelationContextFactory
    {
        CorrelationContext CreateContext(HttpContext httpContext);
    }
}
