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
            CorrelationId newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("123"));

            Assert.Equal("123", newCorrelationId);
        }

        [Fact]
        public void GenerateIfEmpty_WithNonEmpty_ExpectSameValue()
        {
            var correlationId = CorrelationId.FromString("123");
            CorrelationId newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("345"));

            Assert.Equal("123", newCorrelationId);
        }
    }
}
