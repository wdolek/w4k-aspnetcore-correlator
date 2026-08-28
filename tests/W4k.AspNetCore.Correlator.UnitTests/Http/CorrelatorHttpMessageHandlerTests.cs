using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Mocks;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Context.Types;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Http;

public class CorrelatorHttpMessageHandlerTests
{
    private static readonly CorrelationId TestCorrelationId = CorrelationId.FromString("123");

    private readonly Mock<ICorrelationContextAccessor> _correlationContextAccessor;

    public CorrelatorHttpMessageHandlerTests()
    {
        _correlationContextAccessor = Mock.Of<ICorrelationContextAccessor>();
    }

    [Test]
    [MethodDataSource(nameof(GenerateIncomingCorrelationContext))]
    public async Task Forward_PropagateAsPredefined_ExpectPredefinedHeader(CorrelationContext correlationContext)
    {
        // arrange
        var outgoingHeader = "X-MyRequest-Id";
        var propagationSettings = PropagationSettings.PropagateAs(outgoingHeader);

        _correlationContextAccessor
            .CorrelationContext
            .Returns(correlationContext);

        async Task AssertRequest(HttpRequestMessage r)
        {
            await Assert.That(r.Headers.Contains(outgoingHeader)).IsTrue();
            await Assert.That(r.Headers.GetValues(outgoingHeader)).Contains(TestCorrelationId.Value);
        }

        var handler = CreateMessageHandler(propagationSettings, _correlationContextAccessor.Object, AssertRequest);

        // act & assert (via test delegating handler)
        var client = new HttpClient(handler);
        _ = await client.GetAsync("https://www.example.com/");
    }

    [Test]
    public async Task Forward_KeepIncomingHeader_ExpectIncomingHeader()
    {
        // arrange
        var incomingHeader = HttpHeaders.RequestId;
        var propagationSettings = PropagationSettings.KeepIncomingHeaderName();

        _correlationContextAccessor
            .CorrelationContext
            .Returns(new RequestCorrelationContext(TestCorrelationId, incomingHeader));

        var handler = CreateMessageHandler(propagationSettings, _correlationContextAccessor.Object, AssertRequest);

        // act & assert (via test delegating handler)
        var client = new HttpClient(handler);
        _ = await client.GetAsync("https://www.example.com/");
        return;

        async Task AssertRequest(HttpRequestMessage r)
        {
            await Assert.That(r.Headers.Contains(incomingHeader)).IsTrue();
            await Assert.That(r.Headers.GetValues(incomingHeader)).Contains(TestCorrelationId.Value);
        }
    }

    [Test]
    public async Task Forward_KeepIncomingHeaderWithGeneratedCorrelationId_ExpectPredefinedIncomingHeader()
    {
        // arrange
        var incomingHeader = HttpHeaders.RequestId;
        var propagationSettings = PropagationSettings.KeepIncomingHeaderName(incomingHeader);

        _correlationContextAccessor
            .CorrelationContext
            .Returns(new GeneratedCorrelationContext(TestCorrelationId));

        var handler = CreateMessageHandler(propagationSettings, _correlationContextAccessor.Object, AssertRequest);

        // act & assert (via test delegating handler)
        var client = new HttpClient(handler);
        _ = await client.GetAsync("https://www.example.com/");
        return;

        async Task AssertRequest(HttpRequestMessage r)
        {
            await Assert.That(r.Headers.Contains(incomingHeader)).IsTrue();
            await Assert.That(r.Headers.GetValues(incomingHeader)).Contains(TestCorrelationId.Value);
        }
    }

    [Test]
    public async Task Forward_WhenForwardingDisabled_ExpectNoCorrelationInRequestMessage()
    {
        // arrange
        var incomingHeader = HttpHeaders.RequestId;
        var propagationSettings = PropagationSettings.NoPropagation;

        _correlationContextAccessor
            .CorrelationContext
            .Returns(new RequestCorrelationContext(TestCorrelationId, incomingHeader));

        var handler = CreateMessageHandler(propagationSettings, _correlationContextAccessor.Object, AssertRequest);

        // act & assert (via test delegating handler)
        var client = new HttpClient(handler);
        _ = await client.GetAsync("https://www.example.com/");
        return;

        async Task AssertRequest(HttpRequestMessage r)
        {
            foreach (var header in r.Headers)
            {
                await Assert.That(header.Key).IsNotEqualTo(incomingHeader);
                await Assert.That(header.Value).DoesNotContain(TestCorrelationId.Value);
            }
        }
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
        PropagationSettings propagationSettings,
        ICorrelationContextAccessor contextAccessor,
        Func<HttpRequestMessage, Task> assertRequest)
    {
        return new CorrelatorHttpMessageHandler(propagationSettings, contextAccessor)
        {
            InnerHandler = new TestDelegatingHandler(assertRequest)
        };
    }

    private class TestDelegatingHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, Task> _assertRequest;

        public TestDelegatingHandler(Func<HttpRequestMessage, Task> assertRequest)
        {
            _assertRequest = assertRequest;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await _assertRequest(request);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}