using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Context;

/// <summary>
/// Correlation context factory.
/// </summary>
public interface ICorrelationContextFactory
{
    /// <summary>
    /// Create correlation context from given HTTP context.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    /// <returns>
    /// Correlation context.
    /// </returns>
    CorrelationContext CreateContext(HttpContext httpContext);
}