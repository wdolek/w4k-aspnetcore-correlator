using System;
using Microsoft.AspNetCore.Http;
using Moq;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HeaderDictionaryExtensionsShould
    {
        [Fact]
        public void ThrowOnNullFromHeaders()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Mock<IHeaderDictionary>().Object.GetCorrelationHeaderName(null));
        }
    }
}
