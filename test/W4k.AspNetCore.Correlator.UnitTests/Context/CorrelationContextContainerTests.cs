using Microsoft.AspNetCore.Http;
using Moq;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Context
{
    public class CorrelationContextContainerTests
    {
        private readonly Mock<ICorrelationContextFactory> _factory;

        public CorrelationContextContainerTests()
        {
            _factory = new Mock<ICorrelationContextFactory>();
        }

        [Fact]
        public void CreateScope_ExpectCorrelationContextFactoryBeingCalled()
        {
            // arrange
            var httpContext = new DefaultHttpContext();

            // act
            var container = new CorrelationContextContainer(_factory.Object);
            using var scope = container.CreateScope(httpContext);

            // assert
            _factory.Verify(
                f => f.CreateContext(It.Is<HttpContext>(ctx => ctx == httpContext)),
                Times.Once);
        }

        [Fact]
        public void CreateScope_ExpectCorrelationContextPresent()
        {
            // arrange
            var httpContext = new DefaultHttpContext();

            var correlationId = CorrelationId.FromString("123");
            var correlationContext = new RequestCorrelationContext(correlationId, HttpHeaders.CorrelationId);

            _factory
                .Setup(f => f.CreateContext(It.IsAny<HttpContext>()))
                .Returns(correlationContext);

            // act
            var container = new CorrelationContextContainer(_factory.Object);
            using var scope = container.CreateScope(httpContext);

            // assert
            // -> container content
            Assert.Equal(correlationContext, container.CorrelationContext);
            Assert.Equal(correlationId, container.CorrelationContext.CorrelationId);

            // -> scope content
            Assert.Equal(correlationContext, scope.CorrelationContext);
            Assert.Equal(correlationId, scope.CorrelationContext.CorrelationId);
        }

        [Fact]
        public void GetCorrelationContext_WhenContainerNotPopulated_ExpectEmptyCorrelationContext()
        {
            // arrange
            var container = new CorrelationContextContainer(_factory.Object);
            var correlationContext = container.CorrelationContext;

            // assert
            Assert.IsType<EmptyCorrelationContext>(correlationContext);
        }
    }
}
