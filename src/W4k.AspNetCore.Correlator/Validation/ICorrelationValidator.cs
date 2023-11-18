namespace W4k.AspNetCore.Correlator.Validation;

/// <summary>
/// Correlation ID value validator.
/// </summary>
public interface ICorrelationValidator
{
    /// <summary>
    /// Validate header value.
    /// </summary>
    /// <param name="value">Correlation header value.</param>
    /// <returns>
    /// Validation result.
    /// </returns>
    ValidationResult Validate(string? value);
}