using System;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Context.Types;

/// <summary>
/// Correlation context with correlation ID generated because received value was invalid.
/// </summary>
public sealed class RegeneratedCorrelationContext : CorrelationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegeneratedCorrelationContext"/> class.
    /// </summary>
    /// <param name="correlationId">Regenerated correlation ID.</param>
    /// <param name="header">Header name which contained invalid correlation ID.</param>
    /// <param name="validationResult">Validation result of received value.</param>
    public RegeneratedCorrelationContext(
        CorrelationId correlationId,
        string header,
        ValidationResult validationResult)
        : base(correlationId)
    {
        ArgumentNullException.ThrowIfNull(header);
        Header = header;
        ValidationResult = validationResult;
    }

    /// <summary>
    /// Gets request header name which contained invalid correlation ID value.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets validation result of received (invalid) value.
    /// </summary>
    public ValidationResult ValidationResult { get; }
}
