using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace W4k.AspNetCore.Correlator.Extensions
{
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
            // may be already registered
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services.AddSingleton<CorrelatorHttpMessageHandler>();
        }
    }
}
