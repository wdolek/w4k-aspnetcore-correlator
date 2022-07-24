using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Benchmarks.Middleware;

internal class DummyMiddleware
{
    private readonly RequestDelegate _next;

    public DummyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        await _next(httpContext);
    }
}
