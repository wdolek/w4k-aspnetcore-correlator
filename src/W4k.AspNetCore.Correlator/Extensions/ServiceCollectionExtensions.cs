using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace W4k.AspNetCore.Correlator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelator(this IServiceCollection services)
        {
            // may be already registered
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services.AddSingleton<CorrelatorHttpMessageHandler>();
        }
    }
}
