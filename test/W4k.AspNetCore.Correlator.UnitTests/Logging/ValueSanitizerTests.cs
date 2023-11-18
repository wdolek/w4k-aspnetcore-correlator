using Xunit;

namespace W4k.AspNetCore.Correlator.Logging;

public class ValueSanitizerTests
{
    [Fact]
    public void Sanitize_ExpectSanitizedValue()
    {
        // arrange
        var value = "Invalid <value>!";

        // act
        var sanitizedValue = ValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal("Invalid__value__", sanitizedValue);
    }

    [Fact]
    public void Sanitize_ExpectSameReferenceForValidValue()
    {
        // arrange
        var value = "ValidValue";

        // act
        var sanitizedValue = ValueSanitizer.Sanitize(value);

        // assert
        Assert.Same(value, sanitizedValue);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('@')]
    public void Sanitize_ExpectTruncatedValue(char c)
    {
        // arrange
        var value = new string(c, (256) + 1);

        // act
        var sanitizedValue = ValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal(256, sanitizedValue.Length);
    }
}