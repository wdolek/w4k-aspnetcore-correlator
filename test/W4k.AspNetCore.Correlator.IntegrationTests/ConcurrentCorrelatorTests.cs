using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.IntegrationTests.Startup;
using Xunit;
using Xunit.Abstractions;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public sealed class ConcurrentCorrelatorTests : CorrelatorTestsBase<CustomOptionsStartup>
    {
        private static readonly Random Random = new Random(12345);

        private readonly ITestOutputHelper _output;

        public ConcurrentCorrelatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CorrelationCorrectForEachRequest()
        {
            // make sure we make machine sweat
            var concurrency = Environment.ProcessorCount * 10;
            _output.WriteLine($"'Concurrency' level: {concurrency} (number of request tasks to be executed)");

            var tasks = Enumerable
                .Range(0, concurrency)
                .Select(_ => CreateCorrelatedRequest())
                .Select((t) => SendRequest(t.Request, t.CorrelationId))
                .ToArray();

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                var testContext = task.Result;
                Assert.Equal(testContext.Expected, testContext.Header);
                Assert.Equal(testContext.Expected, testContext.Body);
            }
        }

        private static (HttpRequestMessage Request, string CorrelationId) CreateCorrelatedRequest()
        {
            var correlationId = $"IT:{Guid.NewGuid():D}:{DateTime.UtcNow.Ticks}";

            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.Add("X-CID", correlationId);

            return (request, correlationId);
        }

        private async Task<CorrelationTestContext> SendRequest(HttpRequestMessage request, string expected)
        {
            HttpResponseMessage response = await Client.SendAsync(request, CancellationToken.None);

            await Task.Delay(Random.Next(127));

            string headerValue = response.Headers.GetValues("X-CID").FirstOrDefault();
            string bodyValue = await response.Content.ReadAsStringAsync();

            await Task.Delay(Random.Next(255));

            return new CorrelationTestContext
            {
                Header = headerValue,
                Body = bodyValue,
                Expected = expected
            };
        }

        private class CorrelationTestContext
        {
            public string Header { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public string Expected { get; set; } = string.Empty;
        }
    }
}
