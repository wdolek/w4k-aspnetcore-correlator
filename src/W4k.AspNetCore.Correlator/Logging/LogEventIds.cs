using Microsoft.Extensions.Logging;

namespace W4k.AspNetCore.Correlator.Logging
{
    internal static class LogEventIds
    {
        public static readonly EventId CorrelatedRequestBegin =
            new EventId(101, nameof(LoggerExtensions.CorrelatedRequestBegin));

        public static readonly EventId CorrelatedRequestEnd =
            new EventId(102, nameof(LoggerExtensions.CorrelatedRequestEnd));

        public static readonly EventId CorrelationIdMissing =
            new EventId(111, nameof(LoggerExtensions.CorrelationIdMissing));

        public static readonly EventId CorrelationIdReceived =
            new EventId(112, nameof(LoggerExtensions.CorrelationIdReceived));

        public static readonly EventId WritingCorrelatedResponse =
            new EventId(121, nameof(LoggerExtensions.WritingCorrelatedResponse));

        public static readonly EventId ReplacingTraceIdentifier =
            new EventId(122, nameof(LoggerExtensions.ReplacingTraceIdentifier));
    }
}
