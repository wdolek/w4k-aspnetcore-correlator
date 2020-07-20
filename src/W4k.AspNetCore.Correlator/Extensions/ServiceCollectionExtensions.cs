﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions
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
        public static IServiceCollection AddCorrelator(this IServiceCollection services)
        {
            return services.AddCorrelator(_ => { });
        }

        /// <summary>
        /// Adds components required by Correlator to service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configureOptions">Configure options callback.</param>
        /// <returns>
        /// Service collection with components registered.
        /// </returns>
        public static IServiceCollection AddCorrelator(this IServiceCollection services, Action<CorrelatorOptions> configureOptions)
        {
            // may be already registered
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services
                .Configure(configureOptions)
                .AddTransient<CorrelatorHttpMessageHandler>();
        }
    }
}
