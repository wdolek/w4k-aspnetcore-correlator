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

    [Fact]
    public void Sanitize_ExpectTruncatedValue()
    {
        // arrange
        var value = new string('a', (8 * 1024) + 1);

        // act
        var sanitizedValue = ValueSanitizer.Sanitize(value);

        // assert
        Assert.Equal(8 * 1024, sanitizedValue.Length);
    }
}