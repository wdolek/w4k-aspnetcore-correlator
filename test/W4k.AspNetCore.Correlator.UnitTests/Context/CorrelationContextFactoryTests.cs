using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.Context
{
    public class CorrelationContextFactoryTests
    {
        private const string CustomHeader = "X-Custom-Request-Id";

        private readonly CorrelatorOptions _baseOptions;

        public CorrelationContextFactoryTests()
        {
            _baseOptions = new CorrelatorOptions();

            _baseOptions.ReadFrom.Clear();
            _baseOptions.ReadFrom.Add(HttpHeaders.CorrelationId);
            _baseOptions.ReadFrom.Add(CustomHeader);
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
            var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions));
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
            var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions));
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
            var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions));
            var correlationContext = factory.CreateContext(httpContext);

            // assert
            Assert.IsType<EmptyCorrelationContext>(correlationContext);
            Assert.True(correlationContext.CorrelationId.IsEmpty);
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
            var factory = new CorrelationContextFactory(new OptionsWrapper<CorrelatorOptions>(_baseOptions));
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
}
