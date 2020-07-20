using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests
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
        public void NewCorrelationId_CallTwice_ExpectDifferentResult()
        {
            var c1 = CorrelationId.NewCorrelationId();
            var c2 = CorrelationId.NewCorrelationId();

            Assert.NotEqual(CorrelationId.Empty, c1);
            Assert.NotEqual(CorrelationId.Empty, c2);
            Assert.NotEqual(c1, c2);
        }
    }
}
