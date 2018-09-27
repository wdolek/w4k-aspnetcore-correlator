using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HeaderDictionaryExtensionsTests
    {
        [Fact]
        public void GetCorrelationHeaderName_WithNull_ExpectArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Mock<IHeaderDictionary>().Object.GetCorrelationHeaderName(null));
        }

        [Theory]
        [InlineData("X-Correlation-ID", new[] { "X-Correlation-ID" }, "X-Correlation-ID")]
        [InlineData("x-correlation-id", new[] { "X-Correlation-ID" }, "X-Correlation-ID")]
        [InlineData("X-Correlation-ID", new[] { "x-correlation-id" }, "x-correlation-id")]
        [InlineData("X-Request-ID", new[] { "X-Correlation-ID", "X-Request-ID" }, "X-Request-ID")]
        public void GetCorrelationHeaderName_WithKnownHeader_ExpectHeaderName(
            string inputHeader,
            string[] knownHeaders,
            string expected)
        {
            // arrange
            var headers = new HeaderDictionary {
                ["Content-Type"] = "application/json",
                ["Content-Length"] = "100",
                [inputHeader] = "123"
            };

            var readFrom = new List<string>(knownHeaders);

            // act
            var headerName = headers.GetCorrelationHeaderName(readFrom);

            // assert
            Assert.Equal(expected, headerName);
        }

        [Fact]
        public void GetCorrelationHeaderName_WithEmptyHeaders_SkipEmpty()
        {
            // arrange
            var headers = new HeaderDictionary {
                ["X-Correlation-ID"] = string.Empty,
                ["X-Request-ID"] = "123"
            };

            var readFrom = new List<string> { "X-Correlation-ID", "X-Request-ID" };

            // act
            var headerName = headers.GetCorrelationHeaderName(readFrom);

            // assert
            Assert.Equal("X-Request-ID", headerName);
        }

        [Fact]
        public void GetCorrelationHeaderName_WithNoCorrelation_ExpectNull()
        {
            // arrange
            var headers = new HeaderDictionary(0);

            var readFrom = new List<string> { "X-Correlation-ID" };

            // act
            var headerName = headers.GetCorrelationHeaderName(readFrom);

            // assert
            Assert.Null(headerName);
        }

        [Theory]
        [MemberData(nameof(GenerateEmptyHeaders))]
        public void GetCorrelationId_WithEmptyHeaders_ExpectEmpty(IHeaderDictionary headers, string headerName)
        {
            CorrelationId correlationId = headers.GetCorrelationId(headerName);
            Assert.Equal(CorrelationId.Empty, correlationId);
        }

        [Fact]
        public void GetCorrelationId_WithSingleValue_ExpectCorrelationId()
        {
            // arrange
            var headers = new HeaderDictionary {
                ["X-Correlation-ID"] = "123"
            };

            // act
            CorrelationId correlationId = headers.GetCorrelationId("X-Correlation-ID");

            // assert
            Assert.Equal("123", correlationId);
        }

        [Fact]
        public void GetCorrelationId_WithMultipleValues_ExpectFirstCorrelationId()
        {
            // arrange
            var headers = new HeaderDictionary {
                ["X-Correlation-ID"] = new StringValues(new[] { "123", "456", "789" })
            };

            // act
            CorrelationId correlationId = headers.GetCorrelationId("X-Correlation-ID");

            // assert
            Assert.Equal("123", correlationId);
        }

        [Fact]
        public void AddIfNotSet_ExpectHeaderToBeAdded()
        {
            // arrange
            IHeaderDictionary headers = new HeaderDictionary();

            // act
            headers = headers.AddIfNotSet("X-Correlation-ID", "123");

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
            headers = headers.AddIfNotSet("X-Correlation-ID", "999");

            // assert
            Assert.True(headers.ContainsKey("X-Correlation-ID"));
            Assert.Equal("123", headers["X-Correlation-ID"]);
        }

        public static IEnumerable<object[]> GenerateEmptyHeaders()
        {
            yield return new object[] { null, "X-Correlation-ID" };
            yield return new object[] { new HeaderDictionary(0), null };
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
