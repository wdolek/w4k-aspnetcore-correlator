using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Http.Extensions;

public class HttpRequestHeadersExtensionsTests
{
    [Test]
    public async Task AddIfNotSet_ExpectHeaderToBeAdded()
    {
        // arrange
        // (NB! there's no public ctor for `HttpRequestHeaders`)
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "123");

        // assert
        await Assert.That(headers.Contains("X-Correlation-ID")).IsTrue();
        await Assert.That(headers.GetValues("X-Correlation-ID")).Contains("123");
    }

    [Test]
    public async Task AddIfNotSet_HeaderAlreadySet_ExpectKeepingOldValue()
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;
        headers.Add("X-Correlation-ID", "123");

        // act
        // (try to set correlation ID "999")
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "999");

        // assert
        await Assert.That(headers.Contains("X-Correlation-ID")).IsTrue();
        await Assert.That(headers.GetValues("X-Correlation-ID")).Contains("123");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task AddIfNotSet_WhenHeaderNameEmpty_ExpectNoChange(string? headerName)
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet(headerName, "999");

        // assert
        await Assert.That(headers).IsEmpty();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task AddIfNotSet_WhenHeaderValueEmpty_ExpectNoChange(string? headerValue)
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", headerValue);

        // assert
        await Assert.That(headers).IsEmpty();
    }
}