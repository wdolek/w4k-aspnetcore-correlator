using Xunit;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelationIdValueSanitizerTests
{
    [Theory]
    [InlineData("Invalid:<value>!\n", "Invalid:_value_!_")]
    [InlineData("<2345", "_2345")]
    [InlineData("1234>", "1234_")]
    [InlineData("<<3>>", "__3__")]
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
    [InlineData('&')]
    public void Sanitize_ExpectTruncatedValue(char c)
    {
        // arrange
        var value = new string(c, 100);

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal(64, sanitizedValue.Length);
    }
}