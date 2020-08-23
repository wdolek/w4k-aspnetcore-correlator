using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Http
{
    public class CorrelatorHttpMessageHandlerTests
    {
        private static readonly CorrelationId TestCorrelationId = CorrelationId.FromString("123");

        private readonly CorrelatorOptions _baseOptions;
        private readonly Mock<ICorrelationContextAccessor> _correlationContextAccessor;

        public CorrelatorHttpMessageHandlerTests()
        {
            _baseOptions = new CorrelatorOptions();
            _correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
        }

        [Theory]
        [MemberData(nameof(GenerateIncomingCorrelationContext))]
        public async Task Forward_PropagateAsPredefined_ExpectPredefinedHeader(CorrelationContext correlationContext)
        {
            // arrange
            var incomingHeader = HttpHeaders.RequestId;
            var outgoindHeader = "X-MyRequest-Id";

            _baseOptions.Forward = PropagationSettings.PropagateAs(outgoindHeader);
            _correlationContextAccessor
                .Setup(a => a.CorrelationContext)
                .Returns(correlationContext);

            void AssertRequest(HttpRequestMessage r)
            {
                Assert.True(r.Headers.Contains(outgoindHeader));
                Assert.Contains(TestCorrelationId.Value, r.Headers.GetValues(outgoindHeader));
            }

            var handler = CreateMessageHandler(_baseOptions, _correlationContextAccessor, AssertRequest);

            // act & assert (via test delegating handler)
            var client = new HttpClient(handler);
            _ = await client
                .GetAsync("https://www.example.com/")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Forward_KeepIncomingHeader_ExpectIncomingHeader()
        {
            // arrange
            var incomingHeader = HttpHeaders.RequestId;

            _baseOptions.Forward = PropagationSettings.KeepIncomingHeaderName();
            _correlationContextAccessor
                .Setup(a => a.CorrelationContext)
                .Returns(new RequestCorrelationContext(TestCorrelationId, incomingHeader));

            void AssertRequest(HttpRequestMessage r)
            {
                Assert.True(r.Headers.Contains(incomingHeader));
                Assert.Contains(TestCorrelationId.Value, r.Headers.GetValues(incomingHeader));
            };

            var handler = CreateMessageHandler(_baseOptions, _correlationContextAccessor, AssertRequest);

            // act & assert (via test delegating handler)
            var client = new HttpClient(handler);
            _ = await client
                .GetAsync("https://www.example.com/")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Forward_KeepIncomingHeaderWithGeneratedCorrelationId_ExpectPredefinedIncomingHeader()
        {
            // arrange
            var incomingHeader = HttpHeaders.RequestId;

            _baseOptions.Forward = PropagationSettings.KeepIncomingHeaderName(incomingHeader);
            _correlationContextAccessor
                .Setup(a => a.CorrelationContext)
                .Returns(new GeneratedCorrelationContext(TestCorrelationId));

            void AssertRequest(HttpRequestMessage r)
            {
                Assert.True(r.Headers.Contains(incomingHeader));
                Assert.Contains(TestCorrelationId.Value, r.Headers.GetValues(incomingHeader));
            };

            var handler = CreateMessageHandler(_baseOptions, _correlationContextAccessor, AssertRequest);

            // act & assert (via test delegating handler)
            var client = new HttpClient(handler);
            _ = await client
                .GetAsync("https://www.example.com/")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Forward_WhenForwardingDisabled_ExpectNoCorrelationInRequestMessage()
        {
            // arrange
            var incomingHeader = HttpHeaders.RequestId;

            _baseOptions.Forward = PropagationSettings.NoPropagation;
            _correlationContextAccessor
                .Setup(a => a.CorrelationContext)
                .Returns(new RequestCorrelationContext(TestCorrelationId, incomingHeader));

            void AssertRequest(HttpRequestMessage r)
            {
                foreach (var header in r.Headers)
                {
                    Assert.NotEqual(incomingHeader, header.Key);
                    Assert.DoesNotContain(TestCorrelationId.Value, header.Value);
                }
            };

            var handler = CreateMessageHandler(_baseOptions, _correlationContextAccessor, AssertRequest);

            // act & assert (via test delegating handler)
            var client = new HttpClient(handler);
            _ = await client
                .GetAsync("https://www.example.com/")
                .ConfigureAwait(false);
        }

        public static IEnumerable<object[]> GenerateIncomingCorrelationContext()
        {
            yield return new object[]
            {
                new RequestCorrelationContext(TestCorrelationId, HttpHeaders.RequestId)
            };

            yield return new object[]
            {
                new GeneratedCorrelationContext(TestCorrelationId)
            };
        }

        private static CorrelatorHttpMessageHandler CreateMessageHandler(
            CorrelatorOptions options,
            Mock<ICorrelationContextAccessor> contextAccessor,
            Action<HttpRequestMessage> assertRequest)
        {
            var optionsWrapper = new OptionsWrapper<CorrelatorOptions>(options);

            return new CorrelatorHttpMessageHandler(optionsWrapper, contextAccessor.Object)
            {
                InnerHandler = new TestDelegatingHandler(assertRequest)
            };
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            private readonly Action<HttpRequestMessage> _assertRequest;

            public TestDelegatingHandler(Action<HttpRequestMessage> assertRequest)
            {
                _assertRequest = assertRequest;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                _assertRequest(request);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
