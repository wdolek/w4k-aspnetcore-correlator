using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TUnit.Mocks;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;

namespace W4k.AspNetCore.Correlator.Context;

public class CorrelationContextContainerTests
{
    private readonly Mock<ICorrelationContextFactory> _factory;

    public CorrelationContextContainerTests()
    {
        _factory = Mock.Of<ICorrelationContextFactory>();
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
        _factory.CreateContext(httpContext).WasCalled(Times.Once);
    }

    [Test]
    public async Task CreateScope_ExpectCorrelationContextPresent()
    {
        // arrange
        var httpContext = new DefaultHttpContext();

        var correlationId = CorrelationId.FromString("123");
        var correlationContext = new RequestCorrelationContext(correlationId, HttpHeaders.CorrelationId);

        _factory
            .CreateContext(Any<HttpContext>())
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