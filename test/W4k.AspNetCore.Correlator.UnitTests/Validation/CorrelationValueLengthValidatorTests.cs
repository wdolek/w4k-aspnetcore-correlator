using Xunit;

namespace W4k.AspNetCore.Correlator.Validation;

public class CorrelationValueLengthValidatorTests
{
    [Theory]
    [InlineData(" _ ")]
    [InlineData("test")]
    [InlineData("1")]
    [InlineData("123456789")]
    [InlineData("1234567890")]
    public void Validate_WhenMaxLengthIs10_ExpectAllShorterInputsValid(string input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenEmptyInput_ExpectInvalidResult(string input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Equal("Value is null or empty", result.Reason);
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("this_is_very_long_correlation_id_value")]
    public void Validate_WhenInputIsLong_ExpectInvalidResult(string input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Matches(@"Received value of length: \d+, expecting max length 10", result.Reason);
    }
}