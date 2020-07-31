using System;

namespace W4k.AspNetCore.Correlator.Context.Types
{
    public sealed class RequestCorrelationContext : CorrelationContext
    {
        public RequestCorrelationContext(CorrelationId correlationId, string originHeader)
            : base(correlationId)
        {
            Header = originHeader ?? throw new ArgumentNullException(nameof(originHeader));
        }

        public string Header { get; }
    }
}
