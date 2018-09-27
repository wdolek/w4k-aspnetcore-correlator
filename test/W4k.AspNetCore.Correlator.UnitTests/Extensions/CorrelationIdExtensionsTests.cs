using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class CorrelationIdExtensionsTests
    {
        [Fact]
        public void GenerateIfEmpty_WithEmpty_ExpectIdToBeGenerated()
        {
            CorrelationId correlationId = CorrelationId.Empty;
            var newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("123").Value);

            Assert.Equal("123", newCorrelationId);
        }

        [Fact]
        public void GenerateIfEmpty_WithNonEmpty_ExpectSameValue()
        {
            CorrelationId correlationId = CorrelationId.FromString("123").Value;
            var newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("345").Value);

            Assert.Equal("123", newCorrelationId);
        }

        [Fact]
        public void GenerateIfEmpty_WithoutFactory_ExpectSameValue()
        {
            CorrelationId correlationId = CorrelationId.Empty;
            var newCorrelationId = correlationId.GenerateIfEmpty(null);

            Assert.Equal(CorrelationId.Empty, newCorrelationId);
        }
    }
}
