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
        /// Adds components required by Correlator to service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns>
        /// Service collection with components registered.
        /// </returns>
        public static IServiceCollection AddCorrelator(this IServiceCollection services) =>
            services.AddCorrelator(_ => { });

        /// <summary>
        /// Adds components required by Correlator to service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configureOptions">Configure options callback.</param>
        /// <returns>
        /// Service collection with components registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions"/> is <c>null</c>.</exception>
        public static IServiceCollection AddCorrelator(
            this IServiceCollection services,
            Action<CorrelatorOptions> configureOptions)
        {
            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            // TODO: Allow user to register own implementation
            services.AddSingleton<ICorrelationContextFactory, CorrelationContextFactory>();
            services.AddSingleton<ICorrelationEmitter, CorrelationEmitter>();

            services.AddSingleton<CorrelationContextContainer>();

            services.AddSingleton<ICorrelationContextAccessor>(
                sp => sp.GetRequiredService<CorrelationContextContainer>());

            services.AddSingleton<ICorrelationScopeFactory>(
                sp => sp.GetRequiredService<CorrelationContextContainer>());

            return services
                .Configure(configureOptions)
                .AddTransient<CorrelatorHttpMessageHandler>();
        }
    }
}
