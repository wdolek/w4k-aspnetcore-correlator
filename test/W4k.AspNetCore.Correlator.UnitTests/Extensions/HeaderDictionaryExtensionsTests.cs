﻿using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HttpRequestHeadersExtensions
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
    }
}
