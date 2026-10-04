using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;

namespace W4k.AspNetCore.Correlator;

public class RejectPolicyTests : CorrelatorTestsBase<RejectPolicyStartup>
{
    [Test]
    public async Task InvalidCorrelationValue_ExpectBadRequest()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-CID", "this_value_is_too_long");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        string body = await response.Content.ReadAsStringAsync();
        await Assert.That(body).IsEmpty();
    }

    [Test]
    public async Task ValidCorrelationValue_ExpectRequestProcessed()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-CID", "123");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string body = await response.Content.ReadAsStringAsync();
        await Assert.That(body).IsEqualTo("should not be reached");
    }
}
