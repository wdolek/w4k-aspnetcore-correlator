using System;
using Xunit;

namespace W4k.AspNetCore.Correlator.Validation;

public class ValidationResultTests
{
    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    public void Invalid_WhenEmptyReason_Throw(string emptyReason, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => ValidationResult.Invalid(emptyReason));
    }
}
