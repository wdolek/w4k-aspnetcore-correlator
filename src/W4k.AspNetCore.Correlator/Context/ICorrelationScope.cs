using System;

namespace W4k.AspNetCore.Correlator.Context;

/// <summary>
/// Correlation scope.
/// </summary>
internal interface ICorrelationScope : IDisposable
{
    /// <summary>
    /// Gets current correlation context.
    /// </summary>
    CorrelationContext CorrelationContext { get; }
}