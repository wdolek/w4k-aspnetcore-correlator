using Microsoft.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.Extensions
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
            builder.AddHttpMessageHandler<CorrelatorHttpMessageHandler>();
    }
}
