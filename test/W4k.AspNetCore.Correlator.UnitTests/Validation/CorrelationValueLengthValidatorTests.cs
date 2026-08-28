using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Validation;

public class CorrelationValueLengthValidatorTests
{
    [Test]
    [Arguments(" _ ")]
    [Arguments("test")]
    [Arguments("1")]
    [Arguments("123456789")]
    [Arguments("1234567890")]
    public async Task Validate_WhenMaxLengthIs10_ExpectAllShorterInputsValid(string input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task Validate_WhenEmptyInput_ExpectInvalidResult(string? input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Reason).IsEqualTo("Value is null or empty");
    }

    [Test]
    [Arguments("12345678901")]
    [Arguments("this_is_very_long_correlation_id_value")]
    public async Task Validate_WhenInputIsLong_ExpectInvalidResult(string input)
    {
        var validator = new CorrelationValueLengthValidator(10);
        var result = validator.Validate(input);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Reason).Matches(@"Received value of length: \d+, expecting max length 10");
    }
}