using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator.Context;

/// <summary>
/// Correlation scope factory.
/// </summary>
internal interface ICorrelationScopeFactory
{
    /// <summary>
    /// Creates disposable correlation scope using given HTTP context.
    /// </summary>
    /// <param name="httpContext">Current HTTP context.</param>
    /// <returns>
    /// Correlation scope.
    /// </returns>
    ICorrelationScope CreateScope(HttpContext httpContext);
}