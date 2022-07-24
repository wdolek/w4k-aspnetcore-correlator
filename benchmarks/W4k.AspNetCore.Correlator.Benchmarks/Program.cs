using BenchmarkDotNet.Running;

namespace W4k.AspNetCore.Correlator.Benchmarks;

internal class Program
{
    public static void Main(string[] args) =>
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
}
