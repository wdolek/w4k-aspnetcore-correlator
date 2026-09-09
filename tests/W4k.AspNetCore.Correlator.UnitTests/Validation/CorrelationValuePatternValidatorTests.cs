using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Validation;

public class CorrelationValuePatternValidatorTests
{
    private readonly CorrelationValuePatternValidator _validator = new();

    [Test]
    [Arguments("123")]
    [Arguments("abc-DEF_123")]
    [Arguments("|~#+./:=.|")]
    [Arguments("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Validate_WhenValueMatchesPattern_ExpectValid(string input)
    {
        var result = _validator.Validate(input);

        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task Validate_WhenEmptyInput_ExpectInvalidResult(string? input)
    {
        var result = _validator.Validate(input);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Reason).IsEqualTo("Value is null or empty");
    }

    [Test]
    [Arguments("hello world")]
    [Arguments("id;123")]
    [Arguments("id\r\n123")]
    [Arguments("id\x1b[31m")]
    [Arguments("<script>alert('x')</script>")]
    public async Task Validate_WhenValueContainsUnsafeCharacters_ExpectInvalidResult(string input)
    {
        var result = _validator.Validate(input);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Reason).IsEqualTo("Value contains characters outside of the allowed set");
    }

    [Test]
    public async Task Validate_WhenValueIsTooLong_ExpectInvalidResult()
    {
        var result = _validator.Validate(new string('a', CorrelationValuePatternValidator.DefaultMaxLength + 1));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Reason).Matches(
            @"Received value of length: \d+, expecting max length \d+");
    }

    [Test]
    public async Task Validate_WhenCustomMaxLength_ExpectRespected()
    {
        // arrange
        var validator = new CorrelationValuePatternValidator(10);

        // act
        var validResult = validator.Validate("123456789");
        var invalidResult = validator.Validate("12345678901");

        // assert
        await Assert.That(validResult.IsValid).IsTrue();
        await Assert.That(invalidResult.IsValid).IsFalse();
    }
}
