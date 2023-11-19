using Xunit;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelationIdValueSanitizerTests
{
    [Fact]
    public void Sanitize_ExpectSanitizedValue()
    {
        // arrange
        var value = "Invalid:<value>!\n";

        // act
        var sanitizedValue = CorrelationIdValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal("Invalid:_value_!_", sanitizedValue);
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