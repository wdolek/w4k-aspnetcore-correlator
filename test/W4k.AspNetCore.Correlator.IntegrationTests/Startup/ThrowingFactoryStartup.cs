using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

namespace W4k.AspNetCore.Correlator.IntegrationTests.Startup
{
    public class ThrowingFactoryStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelator()
                .WithCorrelationContextFactory<ThrowingCorrelationContextFactory>()
                .WithDefaultCorrelationEmitter();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelator();
            app.Use(async (httpContext, next) => {
                await httpContext.Response.WriteAsync("OK");
            });
        }

        private class ThrowingCorrelationContextFactory : ICorrelationContextFactory
        {
            public CorrelationContext CreateContext(HttpContext httpContext) => throw new Exception();
        }
    }
}
