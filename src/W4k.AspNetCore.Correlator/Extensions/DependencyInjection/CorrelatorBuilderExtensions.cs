using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using W4k.AspNetCore.Correlator.Context;

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions of <see cref="ICorrelatorBuilder"/>.
    /// </summary>
    public static class CorrelatorBuilderExtensions
    {
        /// <summary>
        /// Registers correlation context factory.
        /// </summary>
        /// <remarks>
        /// Provided implementation of correlation context factory is registered as singleton.
        /// </remarks>
        /// <typeparam name="TImpl">Implementing type of correlation context factory.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation context factory second time.</exception>
        public static ICorrelatorBuilder WithCorrelationContextFactory<TImpl>(this ICorrelatorBuilder builder)
            where TImpl : class, ICorrelationContextFactory
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Services.Any(svc => svc.ServiceType == typeof(ICorrelationContextFactory)))
            {
                throw new InvalidOperationException(
                    $"Correlation context factory ({typeof(ICorrelationContextFactory).FullName}) has been already registered, remove default registration or ensure registration happens only once.");
            }

            builder.Services.AddSingleton<ICorrelationContextFactory, TImpl>();

            return builder;
        }

        /// <summary>
        /// Registers default implementation of correlation context factory.
        /// </summary>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        public static ICorrelatorBuilder WithDefaultCorrelationContextFactory(this ICorrelatorBuilder builder) =>
            builder.WithCorrelationContextFactory<CorrelationContextFactory>();

        /// <summary>
        /// Registers correlation emitter.
        /// </summary>
        /// <remarks>
        /// Provided implementation of correlation emitter is registered as singleton.
        /// </remarks>
        /// <typeparam name="TImpl">Implementing type of correlation emitter.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation emitter second time.</exception>
        public static ICorrelatorBuilder WithCorrelationEmitter<TImpl>(this ICorrelatorBuilder builder)
            where TImpl : class, ICorrelationEmitter
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Services.Any(svc => svc.ServiceType == typeof(ICorrelationEmitter)))
            {
                throw new InvalidOperationException(
                    $"Correlation emitter ({typeof(ICorrelationEmitter).FullName}) has been already registered, remove default registration or ensure registration happens only once.");
            }

            builder.Services.AddSingleton<ICorrelationEmitter, TImpl>();

            return builder;
        }

        /// <summary>
        /// Registers correlation emitter.
        /// </summary>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        public static ICorrelatorBuilder WithDefaultCorrelationEmitter(this ICorrelatorBuilder builder) =>
            builder.WithCorrelationEmitter<CorrelationEmitter>();
    }
}
