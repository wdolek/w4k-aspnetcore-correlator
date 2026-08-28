using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelationIdValueSanitizerTests
{
    [Test]
    [Arguments("Invalid:<value>!\n", "Invalid:*value***")]
    [Arguments("<2345", "*2345")]
    [Arguments("1<345", "1*345")]
    [Arguments("123>5", "123*5")]
    [Arguments("1234>", "1234*")]
    [Arguments("<<3>>", "**3**")]
    [Arguments("1<3>5", "1*3*5")]
    [Arguments("X Z", "X*Z")]
    [Arguments("\r \n \t \b", "*******")]
    [Arguments("バトル・ロワイアル", "*********")]
    [Arguments("\"%'()*,?@{}", "***********")]
    public async Task Sanitize_ExpectSanitizedValue(string input, string expected)
    {
        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(input);

        // assert
        await Assert.That(sanitizedValue).IsEqualTo(expected);
    }

    [Test]
    public async Task Sanitize_ExpectSameReferenceForValidValue()
    {
        // arrange
        var value = "ValidValue";

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        await Assert.That(sanitizedValue).IsSameReferenceAs(value);
    }

    [Test]
    [Arguments('a')]
    [Arguments('?')]
    public async Task Sanitize_ExpectTruncatedValue(char c)
    {
        // arrange
        var value = new string(c, 100);

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        await Assert.That(sanitizedValue.Length).IsEqualTo(80);
    }
}