using System;
using Xunit;

namespace W4k.AspNetCore.Correlator.Validation
{
    public class ValidationResultTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Invalid_WhenEmptyReason_Throw(string emptyReason)
        {
            Assert.Throws<ArgumentNullException>(() => ValidationResult.Invalid(emptyReason));
        }
    }
}
