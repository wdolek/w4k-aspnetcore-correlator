using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Context;

namespace W4k.AspNetCore.Correlator.Http;

/// <summary>
/// Correlation ID emitter, handles emitting correlation ID back to caller within response.
/// </summary>
public interface ICorrelationEmitter
{
    /// <summary>
    /// Emits correlation ID back to caller.
    /// </summary>
    /// <param name="httpContext">Current HTTP context.</param>
    /// <param name="correlationContext">Current correlation context.</param>
    /// <returns>
    /// Task representing emit action.
    /// </returns>
    Task Emit(HttpContext httpContext, CorrelationContext correlationContext);
}