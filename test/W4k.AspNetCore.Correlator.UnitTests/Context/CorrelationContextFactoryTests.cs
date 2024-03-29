using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;
using W4k.AspNetCore.Correlator.Validation;
using Xunit;

namespace W4k.AspNetCore.Correlator.Context;

public class CorrelationContextFactoryTests
{
    private const string CustomHeader = "X-Custom-Request-Id";

    private readonly CorrelatorOptions _baseOptions;
    private readonly NullLogger<CorrelationContextFactory> _logger;

    public CorrelationContextFactoryTests()
    {
        _baseOptions = new CorrelatorOptions();

        _baseOptions.ReadFrom.Clear();
        _baseOptions.ReadFrom.Add(HttpHeaders.CorrelationId);
        _baseOptions.ReadFrom.Add(CustomHeader);

        _logger = new NullLogger<CorrelationContextFactory>();
    }

    [Theory]
    [MemberData(nameof(GenerateKnownHeaderNames))]
    public void CreateContext_WhenIncomingHeaderPresent_ExpectRequestCorrelationContext(string incomingHeader)
    {
        // arrange
        var correlationId = "123";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    [incomingHeader] = correlationId,
                }
            }
        };

        // act
        var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions), _logger);
        var correlationContext = factory.CreateContext(httpContext);

        // assert
        var requestCorrelationContext = Assert.IsType<RequestCorrelationContext>(correlationContext);
        Assert.Equal(correlationId, correlationContext.CorrelationId.Value);
        Assert.Equal(incomingHeader, requestCorrelationContext.Header);
    }

    [Fact]
    public void CreateContext_WhenGeneratingCorrelation_ExpectGeneratedCorrelationContext()
    {
        // arrange
        var correlationId = "123";
        var httpContext = new DefaultHttpContext();

        _baseOptions.Factory = _ => CorrelationId.FromString(correlationId);

        // act
        var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions), _logger);
        var correlationContext = factory.CreateContext(httpContext);

        // assert
        Assert.IsType<GeneratedCorrelationContext>(correlationContext);
        Assert.Equal(correlationId, correlationContext.CorrelationId.Value);
    }

    [Fact]
    public void CreateContext_WhenNoFactory_ExpectEmptyCorrelationContext()
    {
        // arrange
        var httpContext = new DefaultHttpContext();

        _baseOptions.Factory = null;

        // act
        var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions), _logger);
        var correlationContext = factory.CreateContext(httpContext);

        // assert
        Assert.IsType<EmptyCorrelationContext>(correlationContext);
        Assert.True(correlationContext.CorrelationId.IsEmpty);
    }

    [Fact]
    public void CreateContext_WhenValidValue_ExpectRequestCorrelationContext()
    {
        // arrange
        var correlationId = "valid_correlation";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    [HttpHeaders.CorrelationId] = correlationId,
                }
            }
        };

        var validationResult = ValidationResult.Valid;
        var validator = new Mock<ICorrelationValidator>();

        validator
            .Setup(v => v.Validate(It.Is<string>(val => string.Equals(correlationId, val))))
            .Returns(validationResult);

        // act
        var factory = new CorrelationContextFactory(
            new OptionsWrapper<CorrelatorOptions>(_baseOptions),
            validator.Object,
            _logger);

        var correlationContext = factory.CreateContext(httpContext);

        // assert
        Assert.IsType<RequestCorrelationContext>(correlationContext);
        Assert.Equal(correlationId, correlationContext.CorrelationId.Value);
    }

    [Fact]
    public void CreateContext_WhenInvalidValue_ExpectInvalidCorrelationContext()
    {
        // arrange
        var correlationId = "invalid_correlation";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    [HttpHeaders.CorrelationId] = correlationId,
                }
            }
        };

        var validationResult = ValidationResult.Invalid("invalid");
        var validator = new Mock<ICorrelationValidator>();

        validator
            .Setup(v => v.Validate(It.Is<string>(val => string.Equals(correlationId, val))))
            .Returns(validationResult);

        // act
        var factory = new CorrelationContextFactory(
            new OptionsWrapper<CorrelatorOptions>(_baseOptions),
            validator.Object,
            _logger);

        var correlationContext = factory.CreateContext(httpContext);

        // assert
        var invalidCorrelationContext = Assert.IsType<InvalidCorrelationContext>(correlationContext);
        Assert.True(correlationContext.CorrelationId.IsEmpty);
        Assert.Equal(HttpHeaders.CorrelationId, invalidCorrelationContext.Header);
        Assert.Equal(invalidCorrelationContext.ValidationResult, validationResult);
    }

    [Fact]
    public void CreateContext_WhenUnknownCorrelationHeader_ExpectEmptyCorrelationContext()
    {
        // arrange
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    ["X-Yet-Another-Custom-Request-Id"] = "123",
                }
            }
        };

        _baseOptions.Factory = null;

        // act
        var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions), _logger);
        var correlationContext = factory.CreateContext(httpContext);

        // assert
        Assert.IsType<EmptyCorrelationContext>(correlationContext);
        Assert.True(correlationContext.CorrelationId.IsEmpty);
    }

    public static IEnumerable<object[]> GenerateKnownHeaderNames()
    {
        yield return new object[] { HttpHeaders.CorrelationId };
        yield return new object[] { CustomHeader };
    }
}