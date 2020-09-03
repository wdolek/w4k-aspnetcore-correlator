using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Startup;
using Xunit;
using Xunit.Abstractions;

namespace W4k.AspNetCore.Correlator
{
    public class ConcurrentCorrelatorTests : CorrelatorTestsBase<ConfiguredCorrelatorStartup>
    {
        private readonly ITestOutputHelper _output;
        private readonly int _concurrency;

        public ConcurrentCorrelatorTests(ITestOutputHelper output)
        {
            _output = output;
            _concurrency = Environment.ProcessorCount * 4;
        }

        [Fact]
        public async Task CorrelationCorrectForEachRequest()
        {
            _output.WriteLine($"'Concurrency' level: {_concurrency} (number of request tasks to be executed)");

            var tasks = Enumerable
                .Range(0, _concurrency)
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

            string headerValue = response.Headers.GetValues("X-CID").FirstOrDefault();
            string bodyValue = await response.Content.ReadAsStringAsync();

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
