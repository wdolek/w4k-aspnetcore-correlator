using Microsoft.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

/// <summary>
/// Correlator builder.
/// </summary>
public interface ICorrelatorBuilder
{
    /// <summary>
    /// Gets services collection.
    /// </summary>
    IServiceCollection Services { get; }
}