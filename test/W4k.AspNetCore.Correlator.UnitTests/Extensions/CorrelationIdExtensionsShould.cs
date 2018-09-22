using Microsoft.AspNetCore.Http;
using Moq;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class CorrelationIdExtensionsShould
    {
        [Fact]
        public void GenerateNewCorrelationIdIfEmpty()
        {
            CorrelationId correlationId = CorrelationId.Empty;
            var newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("123").Value);

            Assert.Equal("123", newCorrelationId);
        }

        [Fact]
        public void NotGenerateNewCorrelationIdIfNotEmpty()
        {
            CorrelationId correlationId = CorrelationId.FromString("123").Value;
            var newCorrelationId = correlationId.GenerateIfEmpty(() => CorrelationId.FromString("345").Value);

            Assert.Equal("123", newCorrelationId);
        }

        [Fact]
        public void NotGenerateNewCorrelationIdIfFactoryNotSet()
        {
            CorrelationId correlationId = CorrelationId.Empty;
            var newCorrelationId = correlationId.GenerateIfEmpty(null);

            Assert.Equal(CorrelationId.Empty, newCorrelationId);
        }

        [Fact]
        public void SetCorrelationIdToHttpContext()
        {
            var context = new Mock<HttpContext>();
            CorrelationId correlationId = CorrelationId.FromString("123").Value;

            correlationId.ApplyTo(context.Object);

            context.VerifySet(c => c.TraceIdentifier = "123", Times.Once);
        }

        [Fact]
        public void NotSetCorrelationIdIfEmpty()
        {
            var context = new Mock<HttpContext>();
            CorrelationId correlationId = CorrelationId.Empty;

            correlationId.ApplyTo(context.Object);

            context.VerifySet(c => c.TraceIdentifier = It.IsAny<string>(), Times.Never);
        }
    }
}
