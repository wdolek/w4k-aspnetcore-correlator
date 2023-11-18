using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.Http;

public class CorrelationEmitterTests
{
    private readonly NullLogger<CorrelationEmitter> _logger;

    public CorrelationEmitterTests()
    {
        _logger = new NullLogger<CorrelationEmitter>();
    }

    [Fact]
    public async Task Emits_WhenKeepingIncomingHeader_ExpectCorrelationIdEmittedWithIncomingHeader()
    {
        // arrange
        var headerName = HttpHeaders.CorrelationId;

        var options = CreateEmitOptions(PropagationSettings.KeepIncomingHeaderName());

        var httpContext = new DefaultHttpContext();
        var correlationContext = new RequestCorrelationContext(
            CorrelationId.FromString("123"),
            headerName);

        // act
        var emitter = new CorrelationEmitter(options, _logger);
        await emitter.Emit(httpContext, correlationContext);

        // assert
        Assert.Contains(headerName, httpContext.Response.Headers);
        Assert.Equal("123", httpContext.Response.Headers[headerName]);
    }

    [Fact]
    public async Task Emits_WhenKeepingIncomingHeaderButCorrelationIdGenerated_ExpectCorrelationIdEmittedWithPredefinedHeader()
    {
        // arrange
        var headerName = "X-Incoming-Request-Id";

        var options = CreateEmitOptions(PropagationSettings.KeepIncomingHeaderName(headerName));

        var httpContext = new DefaultHttpContext();
        var correlationContext = new GeneratedCorrelationContext(CorrelationId.FromString("123"));

        // act
        var emitter = new CorrelationEmitter(options, _logger);
        await emitter.Emit(httpContext, correlationContext);

        // assert
        Assert.Contains(headerName, httpContext.Response.Headers);
        Assert.Equal("123", httpContext.Response.Headers[headerName].ToString());
    }

    [Fact]
    public async Task Emits_WhenEmmitingUsingCustomHeader_ExpectCorrelationIdEmittedWithPredefinedHeader()
    {
        // arrange
        var incomingHeader = HttpHeaders.AspNetRequestId;
        var outgoingHeader = "X-Le-Custom-Request-Id";

        var options = CreateEmitOptions(PropagationSettings.PropagateAs(outgoingHeader));

        var httpContext = new DefaultHttpContext();
        var correlationContext = new RequestCorrelationContext(
            CorrelationId.FromString("123"),
            incomingHeader);

        // act
        var emitter = new CorrelationEmitter(options, _logger);
        await emitter.Emit(httpContext, correlationContext);

        // assert
        Assert.Contains(outgoingHeader, httpContext.Response.Headers);
        Assert.Equal("123", httpContext.Response.Headers[outgoingHeader]);
    }

    [Fact]
    public async Task Emits_WhenNoEmit_ExpectNoCorrelationIdEmitted()
    {
        // arrange
        var incomingHeader = HttpHeaders.AspNetRequestId;

        var options = CreateEmitOptions(PropagationSettings.NoPropagation);

        var httpContext = new DefaultHttpContext();
        var correlationContext = new RequestCorrelationContext(
            CorrelationId.FromString("123"),
            incomingHeader);

        // act
        var emitter = new CorrelationEmitter(options, _logger);
        await emitter.Emit(httpContext, correlationContext);

        // assert
        Assert.DoesNotContain(incomingHeader, httpContext.Response.Headers);
    }

    private static IOptions<CorrelatorOptions> CreateEmitOptions(PropagationSettings emitSettings)
    {
        var options = new CorrelatorOptions
        {
            Emit = emitSettings
        };

        return new OptionsWrapper<CorrelatorOptions>(options);
    }
}