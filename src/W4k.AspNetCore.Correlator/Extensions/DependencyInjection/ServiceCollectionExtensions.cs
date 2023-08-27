using System;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions of <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds default components required by Correlator to services collection.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        public static ICorrelatorBuilder AddDefaultCorrelator(this IServiceCollection services) =>
            services.AddDefaultCorrelator(_ => { });

        /// <summary>
        /// Adds default components required by Correlator to services collection.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="configureOptions">Configure options callback.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        public static ICorrelatorBuilder AddDefaultCorrelator(
            this IServiceCollection services,
            Action<CorrelatorOptions> configureOptions) =>
            services
                .AddCorrelator(configureOptions)
                .WithCorrelationContextFactory<CorrelationContextFactory>()
                .WithCorrelationEmitter<CorrelationEmitter>();

        /// <summary>
        /// Adds components required by Correlator to services collection.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        public static ICorrelatorBuilder AddCorrelator(this IServiceCollection services) =>
            services.AddCorrelator(_ => { });

        /// <summary>
        /// Adds components required by Correlator to services collection.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="configureOptions">Configure options callback.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions"/> is <c>null</c>.</exception>
        public static ICorrelatorBuilder AddCorrelator(
            this IServiceCollection services,
            Action<CorrelatorOptions> configureOptions)
        {
            ThrowHelper.ThrowIfNull(configureOptions, nameof(configureOptions));

            var optionsBuilder = services
                .AddOptions<CorrelatorOptions>()
                .Configure(configureOptions);

#if NET6_0 || NETSTANDARD2_1
            optionsBuilder.Validate(
                options =>
                {
                    return options.ReadFrom.Count > 0;
                },
                $"Configure at least one correlation HTTP header, see property: {nameof(CorrelatorOptions.ReadFrom)}");
#endif

            services
                .AddSingleton<CorrelationContextContainer>()
                .AddSingleton<ICorrelationContextAccessor>(
                    sp => sp.GetRequiredService<CorrelationContextContainer>())
                .AddSingleton<ICorrelationScopeFactory>(
                    sp => sp.GetRequiredService<CorrelationContextContainer>());

            return new CorrelatorBuilder(services);
        }
    }
}
