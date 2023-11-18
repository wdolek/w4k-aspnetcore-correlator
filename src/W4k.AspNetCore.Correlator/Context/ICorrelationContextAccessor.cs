namespace W4k.AspNetCore.Correlator.Context;

/// <summary>
/// Correlation context accessor.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// Gets current correlation context.
    /// </summary>
    CorrelationContext CorrelationContext { get; }
}