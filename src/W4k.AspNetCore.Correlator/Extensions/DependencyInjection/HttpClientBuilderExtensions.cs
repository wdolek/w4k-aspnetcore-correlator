﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Extensions of <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configures HTTP client with <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        /// <param name="builder">HTTP client builder.</param>
        /// <returns>
        /// HTTP client builder configured to use correlator message handler.
        /// </returns>
        public static IHttpClientBuilder WithCorrelation(this IHttpClientBuilder builder) =>
            builder.AddHttpMessageHandler(sp => new CorrelatorHttpMessageHandler(
                sp.GetRequiredService<IOptions<CorrelatorOptions>>().Value.Forward,
                sp.GetRequiredService<ICorrelationContextAccessor>()));

        /// <summary>
        /// Configures HTTP client with configured <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        /// <param name="builder">HTTP client builder.</param>
        /// <param name="propagationSettings">Correlation propagation settings.</param>
        /// <returns>
        /// HTTP client builder configured to use correlator message handler.
        /// </returns>
        public static IHttpClientBuilder WithCorrelation(
            this IHttpClientBuilder builder,
            PropagationSettings propagationSettings) =>
            builder.AddHttpMessageHandler(sp => new CorrelatorHttpMessageHandler(
                propagationSettings,
                sp.GetRequiredService<ICorrelationContextAccessor>()));
    }
}

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions of <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configures HTTP client with <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        /// <param name="builder">HTTP client builder.</param>
        /// <returns>
        /// HTTP client builder configured to use correlator message handler.
        /// </returns>
        [Obsolete("Use extensions from `W4k.AspNetCore.Correlator`.")]
        public static IHttpClientBuilder WithCorrelation(this IHttpClientBuilder builder) =>
            Correlator.HttpClientBuilderExtensions.WithCorrelation(builder);

        /// <summary>
        /// Configures HTTP client with configured <see cref="CorrelatorHttpMessageHandler"/>.
        /// </summary>
        /// <param name="builder">HTTP client builder.</param>
        /// <param name="propagationSettings">Correlation propagation settings.</param>
        /// <returns>
        /// HTTP client builder configured to use correlator message handler.
        /// </returns>
        [Obsolete("Use extensions from `W4k.AspNetCore.Correlator`.")]
        public static IHttpClientBuilder WithCorrelation(
            this IHttpClientBuilder builder,
            PropagationSettings propagationSettings) =>
            Correlator.HttpClientBuilderExtensions.WithCorrelation(builder, propagationSettings);
    }
}