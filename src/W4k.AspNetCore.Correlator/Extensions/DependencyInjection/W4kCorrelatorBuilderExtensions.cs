using System;
using System.Linq;
using W4k.AspNetCore.Correlator;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions of <see cref="ICorrelatorBuilder"/>.
    /// </summary>
    public static class W4kCorrelatorBuilderExtensions
    {
        /// <summary>
        /// Registers correlation context factory.
        /// </summary>
        /// <remarks>
        /// Provided implementation of correlation context factory is registered as singleton.
        /// </remarks>
        /// <typeparam name="T">Implementing type of correlation context factory.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation context factory second time.</exception>
        public static ICorrelatorBuilder WithCorrelationContextFactory<T>(this ICorrelatorBuilder builder)
            where T : class, ICorrelationContextFactory
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (builder.Services.Any(svc => svc.ServiceType == typeof(ICorrelationContextFactory)))
            {
                ThrowHelper.ThrowInvalidOpException(
                    $"Correlation context factory ({typeof(ICorrelationContextFactory).FullName}) has been already registered, remove default registration or ensure registration happens only once.");
            }

            builder.Services.AddSingleton<ICorrelationContextFactory, T>();

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
        /// <typeparam name="T">Implementing type of correlation emitter.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation emitter second time.</exception>
        public static ICorrelatorBuilder WithCorrelationEmitter<T>(this ICorrelatorBuilder builder)
            where T : class, ICorrelationEmitter
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (builder.Services.Any(svc => svc.ServiceType == typeof(ICorrelationEmitter)))
            {
                ThrowHelper.ThrowInvalidOpException(
                    $"Correlation emitter ({typeof(ICorrelationEmitter).FullName}) has been already registered, remove default registration or ensure registration happens only once.");
            }

            builder.Services.AddSingleton<ICorrelationEmitter, T>();

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

        /// <summary>
        /// Registers correlation validator.
        /// </summary>
        /// <typeparam name="T">Implementing type of correlation validator.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <param name="validator">Instance of correlation validator.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="validator"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation validator second time.</exception>
        public static ICorrelatorBuilder WithValidator<T>(this ICorrelatorBuilder builder, T validator)
            where T : ICorrelationValidator
        {
            ArgumentNullException.ThrowIfNull(validator);

            if (builder.Services.Any(svc => svc.ServiceType == typeof(ICorrelationValidator)))
            {
                ThrowHelper.ThrowInvalidOpException(
                    $"Correlation validator ({typeof(ICorrelationValidator).FullName} -> {typeof(T).FullName}) has been already registered, only one validator is supported.");
            }

            builder.Services.AddSingleton<ICorrelationValidator>(validator);

            return builder;
        }
    }
}

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
        /// <typeparam name="T">Implementing type of correlation context factory.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation context factory second time.</exception>
        [Obsolete("Use extensions from `Microsoft.Extensions.DependencyInjection`.")]
        public static ICorrelatorBuilder WithCorrelationContextFactory<T>(this ICorrelatorBuilder builder)
            where T : class, ICorrelationContextFactory =>
            Microsoft.Extensions.DependencyInjection.W4kCorrelatorBuilderExtensions.WithCorrelationContextFactory<T>(builder);

        /// <summary>
        /// Registers default implementation of correlation context factory.
        /// </summary>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        [Obsolete("Use extensions from `Microsoft.Extensions.DependencyInjection`.")]
        public static ICorrelatorBuilder WithDefaultCorrelationContextFactory(this ICorrelatorBuilder builder) =>
            Microsoft.Extensions.DependencyInjection.W4kCorrelatorBuilderExtensions.WithDefaultCorrelationContextFactory(builder);

        /// <summary>
        /// Registers correlation emitter.
        /// </summary>
        /// <remarks>
        /// Provided implementation of correlation emitter is registered as singleton.
        /// </remarks>
        /// <typeparam name="T">Implementing type of correlation emitter.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation emitter second time.</exception>
        [Obsolete("Use extensions from `Microsoft.Extensions.DependencyInjection`.")]
        public static ICorrelatorBuilder WithCorrelationEmitter<T>(this ICorrelatorBuilder builder)
            where T : class, ICorrelationEmitter =>
            Microsoft.Extensions.DependencyInjection.W4kCorrelatorBuilderExtensions.WithCorrelationEmitter<T>(builder);

        /// <summary>
        /// Registers correlation emitter.
        /// </summary>
        /// <param name="builder">Correlator builder.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        [Obsolete("Use extensions from `Microsoft.Extensions.DependencyInjection`.")]
        public static ICorrelatorBuilder WithDefaultCorrelationEmitter(this ICorrelatorBuilder builder) =>
            Microsoft.Extensions.DependencyInjection.W4kCorrelatorBuilderExtensions.WithDefaultCorrelationEmitter(builder);

        /// <summary>
        /// Registers correlation validator.
        /// </summary>
        /// <typeparam name="T">Implementing type of correlation validator.</typeparam>
        /// <param name="builder">Correlator builder.</param>
        /// <param name="validator">Instance of correlation validator.</param>
        /// <returns>
        /// Correlator builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="validator"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to register correlation validator second time.</exception>
        [Obsolete("Use extensions from `Microsoft.Extensions.DependencyInjection`.")]
        public static ICorrelatorBuilder WithValidator<T>(this ICorrelatorBuilder builder, T validator)
            where T : ICorrelationValidator =>
            Microsoft.Extensions.DependencyInjection.W4kCorrelatorBuilderExtensions.WithValidator(builder, validator);
    }
}
