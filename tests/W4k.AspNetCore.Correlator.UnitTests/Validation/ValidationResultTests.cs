using System;

namespace W4k.AspNetCore.Correlator.Validation;

public class ValidationResultTests
{
    [Test]
    [Arguments(null, typeof(ArgumentNullException))]
    [Arguments("", typeof(ArgumentException))]
    public void Invalid_WhenEmptyReason_Throw(string? emptyReason, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => ValidationResult.Invalid(emptyReason!));
    }
}