using Microsoft.AspNetCore.Http;
using Moq;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Context;

public class CorrelationContextContainerTests
{
    private readonly Mock<ICorrelationContextFactory> _factory;

    public CorrelationContextContainerTests()
    {
        _factory = new Mock<ICorrelationContextFactory>();
    }

    [Test]
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

    [Test]
    public async Task CreateScope_ExpectCorrelationContextPresent()
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
        await Assert.That(container.CorrelationContext).IsEqualTo(correlationContext);
        await Assert.That(container.CorrelationContext.CorrelationId).IsEqualTo(correlationId);

        // -> scope content
        await Assert.That(scope.CorrelationContext).IsEqualTo(correlationContext);
        await Assert.That(scope.CorrelationContext.CorrelationId).IsEqualTo(correlationId);
    }

    [Test]
    public async Task GetCorrelationContext_WhenContainerNotPopulated_ExpectEmptyCorrelationContext()
    {
        // arrange
        var container = new CorrelationContextContainer(_factory.Object);
        var correlationContext = container.CorrelationContext;

        // assert
        await Assert.That(correlationContext).IsTypeOf<EmptyCorrelationContext>();
    }
}