using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HeaderDictionaryExtensionsTests
    {
        [Fact]
        public void AddIfNotSet_ExpectHeaderToBeAdded()
        {
            // arrange
            IHeaderDictionary headers = new HeaderDictionary();

            // act
            headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "123");

            // assert
            Assert.True(headers.ContainsKey("X-Correlation-ID"));
            Assert.Equal("123", headers["X-Correlation-ID"]);
        }

        [Fact]
        public void AddIfNotSet_HeaderAlreadySet_ExpectKeepingOldValue()
        {
            // arrange
            IHeaderDictionary headers = new HeaderDictionary
            {
                ["X-Correlation-ID"] = "123"
            };

            // act
            // (try to set correlation ID "999")
            headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "999");

            // assert
            Assert.True(headers.ContainsKey("X-Correlation-ID"));
            Assert.Equal("123", headers["X-Correlation-ID"]);
        }

        public static IEnumerable<object[]> GenerateEmptyHeaders()
        {
            yield return new object[] { new HeaderDictionary(0), "X-Correlation-ID" };

            // silly header value (can this even happen?!)
            yield return new object[]
            {
                new HeaderDictionary
                {
                    ["X-Correlation-ID"] = StringValues.Empty
                },
                "X-Correlation-ID"
            };
        }
    }
}
