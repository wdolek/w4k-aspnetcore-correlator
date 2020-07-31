using System.Threading;

namespace W4k.AspNetCore.Correlator.Context
{
    internal class CorrelationContextContainer : ICorrelationContextAccessor, ICorrelationContextContainer
    {
        private static readonly AsyncLocal<CorrelationContext?> LocalContext = new AsyncLocal<CorrelationContext?>();

        public CorrelationContext? CorrelationContext => LocalContext.Value;

        public CorrelationScope CreateScope(CorrelationContext correlationContext)
        {
            LocalContext.Value = correlationContext;

            return new CorrelationScope(LocalContext);
        }
    }
}
