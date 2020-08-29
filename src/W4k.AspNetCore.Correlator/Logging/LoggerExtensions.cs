using System;
using Microsoft.Extensions.Logging;

namespace W4k.AspNetCore.Correlator.Logging
{
    internal static partial class LoggerExtensions
    {
        private static readonly EventId CorrelatedRequestBeginEvent =
            new EventId(101, nameof(LoggerExtensions.CorrelatedRequestBegin));

        private static readonly EventId CorrelatedRequestEndEvent =
            new EventId(102, nameof(LoggerExtensions.CorrelatedRequestEnd));

        private static readonly EventId ReplacingTraceIdentifierEvent =
            new EventId(103, nameof(LoggerExtensions.ReplacingTraceIdentifier));

        private static readonly Action<ILogger, Exception> LogCorrelatedRequestBegin =
            LoggerMessage.Define(
                LogLevel.Debug,
                CorrelatedRequestBeginEvent,
                "Correlated request started");

        private static readonly Action<ILogger, Exception> LogCorrelatedRequestEnd =
            LoggerMessage.Define(
                LogLevel.Debug,
                CorrelatedRequestEndEvent,
                "Correlated request finished");

        private static readonly Action<ILogger, string, Exception> LogReplacingTraceIdentifier =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                ReplacingTraceIdentifierEvent,
                "Replacing TraceIdentifier ({TraceIdentifier}) by correlation ID");

        public static void CorrelatedRequestBegin(this ILogger logger) =>
            LogCorrelatedRequestBegin(logger, null!);

        public static void CorrelatedRequestEnd(this ILogger logger) =>
            LogCorrelatedRequestEnd(logger, null!);

        public static void ReplacingTraceIdentifier(this ILogger logger, string traceIdentifier) =>
            LogReplacingTraceIdentifier(logger, traceIdentifier, null!);
    }
}
