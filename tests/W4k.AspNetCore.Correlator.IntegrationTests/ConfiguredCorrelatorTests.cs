using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;

namespace W4k.AspNetCore.Correlator;

public class ConfiguredCorrelatorTests : CorrelatorTestsBase<ConfiguredCorrelatorStartup>
{
    [Test]
    public async Task CorrelationIdReadFromRequest()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // incoming header name is customized in startup
        request.Headers.Add("X-CID", "123");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.Contains("X-CID")).IsTrue();

        string correlationIdEmitted = response.Headers.GetValues("X-CID").First();
        await Assert.That(correlationIdEmitted).IsEqualTo("123");

        string correlationId = await response.Content.ReadAsStringAsync();
        await Assert.That(correlationId).IsEqualTo("123");
    }
}