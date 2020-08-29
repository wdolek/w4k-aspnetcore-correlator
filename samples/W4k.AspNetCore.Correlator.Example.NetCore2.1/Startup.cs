using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Example.NetCore21
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelator(options =>
            {
                options.Forward = PropagationSettings.PropagateAs("X-Correlation-Id");
                options.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
                options.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("correlation");
            });

            services
                .AddHttpClient("DummyClient")
                .WithCorrelation();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCorrelator();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
