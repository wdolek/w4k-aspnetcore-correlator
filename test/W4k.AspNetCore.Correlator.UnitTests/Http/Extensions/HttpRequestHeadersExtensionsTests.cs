using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace W4k.AspNetCore.Correlator.Http.Extensions;

public class HttpRequestHeadersExtensionsTests
{
    [Fact]
    public void AddIfNotSet_ExpectHeaderToBeAdded()
    {
        // arrange
        // (NB! there's no public ctor for `HttpRequestHeaders`)
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "123");

        // assert
        Assert.True(headers.Contains("X-Correlation-ID"));
        Assert.Contains("123", headers.GetValues("X-Correlation-ID"));
    }

    [Fact]
    public void AddIfNotSet_HeaderAlreadySet_ExpectKeepingOldValue()
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;
        headers.Add("X-Correlation-ID", "123");

        // act
        // (try to set correlation ID "999")
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "999");

        // assert
        Assert.True(headers.Contains("X-Correlation-ID"));
        Assert.Contains("123", headers.GetValues("X-Correlation-ID"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddIfNotSet_WhenHeaderNameEmpty_ExpectNoChange(string? headerName)
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet(headerName, "999");

        // assert
        Assert.Empty(headers);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddIfNotSet_WhenHeaderValueEmpty_ExpectNoChange(string? headerValue)
    {
        // arrange
        HttpRequestHeaders headers = new HttpClient().DefaultRequestHeaders;

        // act
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", headerValue);

        // assert
        Assert.Empty(headers);
    }
}