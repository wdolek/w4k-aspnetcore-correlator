namespace W4k.AspNetCore.Correlator.Context.Types
{
    public sealed class EmptyCorrelationContext : CorrelationContext
    {
        public static readonly EmptyCorrelationContext Instance = new EmptyCorrelationContext();

        private EmptyCorrelationContext()
            : base(CorrelationId.Empty)
        {
        }
    }
}
