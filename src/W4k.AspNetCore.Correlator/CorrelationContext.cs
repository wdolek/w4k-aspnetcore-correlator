using System;

namespace W4k.AspNetCore.Correlator
{
    public sealed class CorrelationContext
    {
        public static readonly CorrelationContext Empty = new CorrelationContext(
            origin: CorrelationOrigin.None,
            headerName: string.Empty,
            correlationId: CorrelationId.Empty);

        private CorrelationContext(CorrelationOrigin origin, string headerName, CorrelationId correlationId)
        {
            Origin = origin;
            HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        }

        public CorrelationOrigin Origin { get; }

        public CorrelationId CorrelationId { get; }

        public string HeaderName { get; }

        public static CorrelationContext FromRequest(string headerName, CorrelationId correlationId) =>
            new CorrelationContext(CorrelationOrigin.Request, headerName, correlationId);

        public static CorrelationContext FromFactory(string headerName, CorrelationId correlationId) =>
            new CorrelationContext(CorrelationOrigin.Factory, headerName, correlationId);
    }
}
