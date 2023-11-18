using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Context.Types;

namespace W4k.AspNetCore.Correlator.Context;

internal class CorrelationContextContainer
    : ICorrelationScopeFactory, ICorrelationContextAccessor, IDisposable
{
    private static readonly AsyncLocal<CorrelationContext?> LocalContext = new();

    private readonly ICorrelationContextFactory _correlationContextFactory;

    public CorrelationContextContainer(ICorrelationContextFactory correlationContextFactory)
    {
        _correlationContextFactory = correlationContextFactory;
    }

    public CorrelationContext CorrelationContext => LocalContext.Value ?? EmptyCorrelationContext.Instance;

    public ICorrelationScope CreateScope(HttpContext httpContext)
    {
        LocalContext.Value = _correlationContextFactory.CreateContext(httpContext);

        return new CorrelationScope(this);
    }

    public void Dispose()
    {
        LocalContext.Value = null;
    }
}