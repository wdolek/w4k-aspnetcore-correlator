using System;
using Microsoft.AspNetCore.Builder;

namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Extensions of <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures application builder to use correlator middleware.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <returns>
        /// Application builder with correlator middleware registered.
        /// </returns>
        public static IApplicationBuilder UseCorrelator(this IApplicationBuilder app) =>
            app.UseMiddleware<CorrelatorMiddleware>();
    }
}

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures application builder to use correlator middleware.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <returns>
        /// Application builder with correlator middleware registered.
        /// </returns>
        [Obsolete("Use extensions from `W4k.AspNetCore.Correlator`.")]
        public static IApplicationBuilder UseCorrelator(this IApplicationBuilder app) =>
            W4k.AspNetCore.Correlator.ApplicationBuilderExtensions.UseCorrelator(app);
    }
}
