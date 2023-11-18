using System;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Context.Types;

/// <summary>
/// Invalid correlation context.
/// </summary>
public sealed class InvalidCorrelationContext : CorrelationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCorrelationContext"/> class.
    /// </summary>
    /// <param name="header">Header name containing correlation ID.</param>
    /// <param name="validationResult">Validation result.</param>
    public InvalidCorrelationContext(string header, ValidationResult validationResult)
        : base(CorrelationId.Empty)
    {
        ArgumentNullException.ThrowIfNull(header);
        Header = header;
        ValidationResult = validationResult;
    }

    /// <summary>
    /// Gets request header name which contained correlation ID value.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets header value validation result.
    /// </summary>
    public ValidationResult ValidationResult { get; }
}
