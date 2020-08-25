using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.IntegrationTests.Startup;
using Xunit;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public class UnbreakableCorrelatorTests : CorrelatorTestsBase<ThrowingFactoryStartup>
    {
        [Fact]
        public async Task Invoke_WhenFactoryThrows_RequestIsProcessed()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add(HttpHeaders.AspNetRequestId, "123");

            // act
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            // assert
            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.Equal("OK", responseBody);
        }
    }
}
