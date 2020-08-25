using Xunit;

namespace W4k.AspNetCore.Correlator
{
    public class CorrelationIdTests
    {
        [Fact]
        public void StringTypeCast_ExpectInternalValue()
        {
            var correlationId = CorrelationId.FromString("123");
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
            var c1 = CorrelationId.FromString(right);
            var c2 = CorrelationId.FromString(left);

            Assert.True(c1.Equals(c2));
            Assert.True(c1 == c2);
            Assert.Equal(c1, c2);
        }

        [Theory]
        [InlineData("test_1", "TEST_2")]
        public void NotEqual_MultipleOverrides_ExpectToBeNotEqual(string right, string left)
        {
            var c1 = CorrelationId.FromString(right);
            var c2 = CorrelationId.FromString(left);

            Assert.True(!c1.Equals(c2));
            Assert.True(c1 != c2);
            Assert.NotEqual(c1, c2);
        }
        [Fact]
        public void Empty_ExpectTrueIfEmpty()
        {
            var correlationId = CorrelationId.Empty;

            Assert.True(correlationId.IsEmpty);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void FromEmptyString_ExpectTrueIfEmpty(string value)
        {
            var correlationId = CorrelationId.FromString(value);

            Assert.True(correlationId.IsEmpty);
        }

        [Fact]
        public void FromString_ExpectNotToBeEmpty()
        {
            var correlationId = CorrelationId.FromString("123");

            Assert.False(correlationId.IsEmpty);
        }
    }
}
