using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;
using Xunit;

namespace W4k.AspNetCore.Correlator;

public class ConfiguredCorrelatorTests : CorrelatorTestsBase<ConfiguredCorrelatorStartup>
{
    [Fact]
    public async Task CorrelationIdReadFromRequest()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // incoming header name is customized in startup
        request.Headers.Add("X-CID", "123");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-CID"));

        string correlationIdEmitted = response.Headers.GetValues("X-CID").First();
        Assert.Equal("123", correlationIdEmitted);

        string correlationId = await response.Content.ReadAsStringAsync();
        Assert.Equal("123", correlationId);
    }
}