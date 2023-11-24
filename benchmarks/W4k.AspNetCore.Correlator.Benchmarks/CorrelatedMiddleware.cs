using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Context;

namespace W4k.AspNetCore.Correlator.Benchmarks;

internal class CorrelatedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public CorrelatedMiddleware(RequestDelegate next, ICorrelationContextAccessor correlationContextAccessor)
    {
        _next = next;
        _correlationContextAccessor = correlationContextAccessor;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        _ = _correlationContextAccessor.CorrelationContext;
        await _next(httpContext);
    }
}