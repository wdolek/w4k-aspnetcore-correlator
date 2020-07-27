namespace W4k.AspNetCore.Correlator.Context
{
    public class EmptyCorrelationContext : CorrelationContext
    {
        public static readonly EmptyCorrelationContext Instance = new EmptyCorrelationContext();

        private EmptyCorrelationContext()
            : base(CorrelationId.Empty)
        {
        }
    }
}
