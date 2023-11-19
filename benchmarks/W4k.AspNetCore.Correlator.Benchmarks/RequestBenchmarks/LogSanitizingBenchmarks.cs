using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using W4k.AspNetCore.Correlator.Benchmarks.Helpers;
using W4k.AspNetCore.Correlator.Benchmarks.Startup;

namespace W4k.AspNetCore.Correlator.Benchmarks.RequestBenchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[MemoryDiagnoser]
public class LogSanitizingBenchmarks : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly string[] _correlationIds;

    [ParamsAllValues]
    public CorrelationIdLength Length { get; set; }

    [ParamsAllValues]
    public CorrelationIdContent Content { get; set; }

    public LogSanitizingBenchmarks()
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<DefaultCorrelatorStartup>());
        _client = _server.CreateClient();

        _correlationIds = new string[1024];

        Randomizer.Seed = new Random(5552368);
        var faker = new Faker();

        for (int i = 0; i < _correlationIds.Length; i++)
        {
            _correlationIds[i] = GenerateCorrelationId(faker, Length, Content);
        }
    }

    [Benchmark]
    public async Task CorrelatedRequest()
    {
        var correlationIds = _correlationIds;
        foreach (var correlationId in correlationIds)
        {
            var requestMessage = RequestFactory.CreateCorrelatedRequest(correlationId);
            await _client.SendAsync(requestMessage);
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }

    private static string GenerateCorrelationId(
        Faker faker,
        CorrelationIdLength correlationIdLength,
        CorrelationIdContent correlationIdContent)
    {
        var length = correlationIdLength == CorrelationIdLength.Short
            ? faker.Random.Int(8, 16)
            : faker.Random.Int(256, 512);

        return correlationIdContent == CorrelationIdContent.OnlyAllowedChars
            ? faker.Random.AlphaNumeric(length)
            : faker.Random.String2(length);
    }

    public enum CorrelationIdLength
    {
        Short,
        Long,
    }

    public enum CorrelationIdContent
    {
        OnlyAllowedChars,
        IncludesInvalidChars,
    }
}