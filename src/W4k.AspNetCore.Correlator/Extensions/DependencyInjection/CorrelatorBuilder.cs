using Microsoft.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

internal class CorrelatorBuilder : ICorrelatorBuilder
{
    public CorrelatorBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}