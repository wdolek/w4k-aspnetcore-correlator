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
        /// Services collection.
        /// </returns>
        public static IServiceCollection AddDefaultCorrelator(this IServiceCollection services) =>
            services.AddDefaultCorrelator(_ => { });

        /// <summary>
        /// Adds default components required by Correlator to services collection.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="configureOptions">Configure options callback.</param>
        /// <returns>
        /// Services collection.
        /// </returns>
        public static IServiceCollection AddDefaultCorrelator(
            this IServiceCollection services,
            Action<CorrelatorOptions> configureOptions) =>
            services
                .AddCorrelator(configureOptions)
                .WithCorrelationContextFactory<CorrelationContextFactory>()
                .WithCorrelationEmitter<CorrelationEmitter>()
                .Services;

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
            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddOptions<CorrelatorOptions>()
                .Configure(configureOptions)
                .Validate(o =>
                {
                    return o.ReadFrom.Count > 0;
                });

            services.AddTransient<CorrelatorHttpMessageHandler>()
                .AddSingleton<CorrelationContextContainer>()
                .AddSingleton<ICorrelationContextAccessor>(
                    sp => sp.GetRequiredService<CorrelationContextContainer>())
                .AddSingleton<ICorrelationScopeFactory>(
                    sp => sp.GetRequiredService<CorrelationContextContainer>());

            return new CorrelatorBuilder(services);
        }
    }
}
