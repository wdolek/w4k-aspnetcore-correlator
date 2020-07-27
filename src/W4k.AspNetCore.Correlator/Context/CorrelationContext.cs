using System;

namespace W4k.AspNetCore.Correlator.Context
{
    public abstract class CorrelationContext
    {
        protected CorrelationContext(CorrelationId correlationId)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        }

        public CorrelationId CorrelationId { get; }
    }
}
