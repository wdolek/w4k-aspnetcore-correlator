using Xunit;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelationIdValueSanitizerTests
{
    [Theory]
    [InlineData("Invalid:<value>!\n", "Invalid:*value***")]
    [InlineData("<2345", "*2345")]
    [InlineData("1<345", "1*345")]
    [InlineData("123>5", "123*5")]
    [InlineData("1234>", "1234*")]
    [InlineData("<<3>>", "**3**")]
    [InlineData("1<3>5", "1*3*5")]
    [InlineData("X Z", "X*Z")]
    [InlineData("\r \n \t \b", "*******")]
    [InlineData("バトル・ロワイアル", "*********")]
    [InlineData("\"%'()*,?@{}", "***********")]
    public void Sanitize_ExpectSanitizedValue(string input, string expected)
    {
        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(input);

        // assert
        Assert.Equal(expected, sanitizedValue);
    }

    [Fact]
    public void Sanitize_ExpectSameReferenceForValidValue()
    {
        // arrange
        var value = "ValidValue";

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        Assert.Same(value, sanitizedValue);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('?')]
    public void Sanitize_ExpectTruncatedValue(char c)
    {
        // arrange
        var value = new string(c, 100);

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal(80, sanitizedValue.Length);
    }
}