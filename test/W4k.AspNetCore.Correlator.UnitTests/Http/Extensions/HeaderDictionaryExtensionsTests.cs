using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Http.Extensions;

public class HttpRequestHeadersExtensions
{
    [Test]
    public async Task AddIfNotSet_ExpectHeaderToBeAdded()
    {
        // arrange
        IHeaderDictionary headers = new HeaderDictionary();

        // act
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "123");

        // assert
        await Assert.That(headers.ContainsKey("X-Correlation-ID")).IsTrue();
        await Assert.That(headers["X-Correlation-ID"].ToString()).IsEqualTo("123");
    }

    [Test]
    public async Task AddIfNotSet_HeaderAlreadySet_ExpectKeepingOldValue()
    {
        // arrange
        IHeaderDictionary headers = new HeaderDictionary
        {
            ["X-Correlation-ID"] = "123"
        };

        // act
        // (try to set correlation ID "999")
        headers = headers.AddHeaderIfNotSet("X-Correlation-ID", "999");

        // assert
        await Assert.That(headers.ContainsKey("X-Correlation-ID")).IsTrue();
        await Assert.That(headers["X-Correlation-ID"].ToString()).IsEqualTo("123");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task AddIfNoSet_WhenHeaderNameIsEmpty_ExpectNoChange(string? headerName)
    {
        // arrange
        IHeaderDictionary headers = new HeaderDictionary();

        // act
        headers = headers.AddHeaderIfNotSet(headerName, "123");

        // assert
        await Assert.That(headers).IsEmpty();
    }
}