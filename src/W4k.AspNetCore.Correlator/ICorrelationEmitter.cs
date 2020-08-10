using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Correlation ID emitter, handles emitting correlation ID back to caller within response.
    /// </summary>
    public interface ICorrelationEmitter
    {
        /// <summary>
        /// Emits correlation ID back to caller.
        /// </summary>
        /// <param name="httpContext">Current HTTP context.</param>
        /// <returns>
        /// Task representing emit action.
        /// </returns>
        Task Emit(HttpContext httpContext);
    }
}
