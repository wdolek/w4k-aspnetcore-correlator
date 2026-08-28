using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;

namespace W4k.AspNetCore.Correlator;

public class DefaultCorrelatorTests : CorrelatorTestsBase<DefaultCorrelatorStartup>
{
    [Test]
    [Arguments("Request-Id")]
    [Arguments("X-Correlation-ID")]
    [Arguments("x-correlation-id")]
    [Arguments("X-Request-ID")]
    public async Task CorrelationIdReadFromRequest(string correlationHeaderName)
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add(correlationHeaderName, "123");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string correlationId = await response.Content.ReadAsStringAsync();
        await Assert.That(correlationId).IsEqualTo("123");
    }

    [Test]
    public async Task CorrelationIdGenerated()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string correlationId = await response.Content.ReadAsStringAsync();
        await Assert.That(correlationId).IsNotEmpty();
        await Assert.That(Guid.TryParse(correlationId, out Guid _)).IsTrue();
    }

    [Test]
    public async Task CorrelationIdNotFound()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Dummy-Correlation-Id", "123");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.Contains("X-Dummy-Correlation-Id")).IsFalse();

        string correlationId = await response.Content.ReadAsStringAsync();
        await Assert.That(correlationId).IsNotEqualTo("123");
        await Assert.That(Guid.TryParse(correlationId, out Guid _)).IsTrue();
    }
}