using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace W4k.AspNetCore.Correlator.IntegrationTests
{
    public abstract class CorrelatorTestsBase<TStartup> : IDisposable
        where TStartup : class
    {
        protected CorrelatorTestsBase()
        {
            IWebHostBuilder builder = new WebHostBuilder()
                .UseEnvironment("test")
                .UseStartup<TStartup>();

            Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        protected TestServer Server { get; }
        protected HttpClient Client { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server?.Dispose();
                Client?.Dispose();
            }
        }
    }
}
