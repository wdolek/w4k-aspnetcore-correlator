using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;

namespace W4k.AspNetCore.Correlator;

public class GenerateNewPolicyTests : CorrelatorTestsBase<GenerateNewPolicyStartup>
{
    [Test]
    public async Task InvalidCorrelationValue_ExpectNewCorrelationIdGenerated()
    {
        // arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-CID", "this_value_is_too_long");

        // act
        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

        // assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string correlationId = await response.Content.ReadAsStringAsync();
        await Assert.That(correlationId).IsNotEmpty();
        await Assert.That(correlationId).IsNotEqualTo("this_value_is_too_long");

        // regenerated correlation ID is emitted with incoming header name
        await Assert.That(response.Headers.Contains("X-CID")).IsTrue();
        await Assert.That(response.Headers.GetValues("X-CID").First()).IsEqualTo(correlationId);
    }
}
