namespace W4k.AspNetCore.Correlator.Validation
{
    /// <summary>
    /// Correlation value length validator: only criteria for valid correlation ID is its length.
    /// </summary>
    /// <remarks>
    /// Empty value is considered invalid.
    /// </remarks>
    public class CorrelationValueLengthValidator : ICorrelationValidator
    {
        private readonly ushort _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationValueLengthValidator"/> class.
        /// </summary>
        /// <param name="length">Maximum length of correlation ID.</param>
        public CorrelationValueLengthValidator(ushort length)
        {
            _length = length;
        }

        /// <inheritdoc/>
        public ValidationResult Validate(string? value)
        {
            if (value is null)
            {
                return ValidationResult.Invalid("Given value is null");
            }

            return value.Length <= _length
                ? ValidationResult.Valid
                : ValidationResult.Invalid($"Received value of length: {value.Length}, expecting max length {_length}");
        }
    }
}
