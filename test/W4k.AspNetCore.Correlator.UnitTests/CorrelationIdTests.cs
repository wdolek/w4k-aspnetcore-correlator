﻿using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests
{
    public class CorrelationIdTests
    {
        [Fact]
        public void Ctor_InstantiatedUsingDefault_ExpectEmpty()
        {
            var correlationId = default(CorrelationId);
            Assert.Equal(string.Empty, correlationId.Value);
        }

        [Fact]
        public void StringTypeCast_ExpectInternalValue()
        {
            CorrelationId correlationId = CorrelationId.FromString("123").Value;
            string value = correlationId;

            Assert.NotNull(value);
            Assert.Equal("123", value);
        }

        [Theory]
        [InlineData("123", "123")]
        [InlineData("test", "test")]
        [InlineData("test", "TEST")]
        public void Equals_MultipleOverrides_ExpectToBeEqual(string right, string left)
        {
            CorrelationId c1 = CorrelationId.FromString(right).Value;
            CorrelationId c2 = CorrelationId.FromString(left).Value;

            Assert.True(c1.Equals(c2));
            Assert.True(c1 == c2);
            Assert.Equal(c1, c2);
        }

        [Theory]
        [InlineData("test_1", "TEST_2")]
        public void NotEqual_MultipleOverrides_ExpectToBeNotEqual(string right, string left)
        {
            CorrelationId c1 = CorrelationId.FromString(right).Value;
            CorrelationId c2 = CorrelationId.FromString(left).Value;

            Assert.True(!c1.Equals(c2));
            Assert.True(c1 != c2);
            Assert.NotEqual(c1, c2);
        }

        [Fact]
        public void FromString_WithValidInput_ExpectCorrelationIdWithInternalValue()
        {
            CorrelationId? correlationId = CorrelationId.FromString("123");
            Assert.NotNull(correlationId);
        }

        [Fact]
        public void FromString_WithInvalidInput_ExpectNull()
        {
            CorrelationId? correlationId = CorrelationId.FromString(null);
            Assert.Null(correlationId);
        }

        [Fact]
        public void NewCorrelationId_CallTwice_ExpectDifferentResult()
        {
            CorrelationId c1 = CorrelationId.NewCorrelationId();
            CorrelationId c2 = CorrelationId.NewCorrelationId();

            Assert.NotEqual(CorrelationId.Empty, c1);
            Assert.NotEqual(CorrelationId.Empty, c2);
            Assert.NotEqual(c1, c2);
        }
    }
}