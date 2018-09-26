using Microsoft.AspNetCore.Http;
using W4k.AspNetCore.Correlator.Extensions;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class HttpContextExtensionsShould
    {
        [Fact]
        public void SetCorrelationIdToHttpContext()
        {
            HttpContext context = new DefaultHttpContext();
            CorrelationId correlationId = CorrelationId.FromString("123").Value;

            context = context.WithCorrelationId(correlationId);

            Assert.Equal(correlationId.Value, context.TraceIdentifier);
        }

        [Fact]
        public void NotSetCorrelationIdIfEmpty()
        {
            HttpContext context = new DefaultHttpContext();
            CorrelationId correlationId = CorrelationId.Empty;

            context = context.WithCorrelationId(correlationId);

            Assert.NotEqual(correlationId.Value, context.TraceIdentifier);
        }
    }
}
