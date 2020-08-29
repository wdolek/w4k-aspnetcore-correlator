using System;
using Microsoft.Extensions.Logging;

namespace W4k.AspNetCore.Correlator.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _correlatedRequestBegin =
            LoggerMessage.Define(
                LogLevel.Debug,
                LogEventIds.CorrelatedRequestBegin,
                "Correlated request started");

        private static readonly Action<ILogger, Exception> _correlatedRequestEnd =
            LoggerMessage.Define(
                LogLevel.Debug,
                LogEventIds.CorrelatedRequestEnd,
                "Correlated request finished");

        private static readonly Action<ILogger, string, Exception> _correlationIdMissing =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                LogEventIds.CorrelationIdMissing,
                "No correlation ID received/generated, request trace: {TraceIdentifier}");

        private static readonly Action<ILogger, CorrelationId, Exception> _correlationIdReceived =
            LoggerMessage.Define<CorrelationId>(
                LogLevel.Information,
                LogEventIds.CorrelationIdReceived,
                "Correlation ID received/generated, correlation: {CorrelationId}");

        private static readonly Action<ILogger, Exception> _writingCorrelatedResponse =
            LoggerMessage.Define(
                LogLevel.Debug,
                LogEventIds.WritingCorrelatedResponse,
                "Writing correlation ID to response headers");

        private static readonly Action<ILogger, string, Exception> _replacingTraceIdentifier =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                LogEventIds.ReplacingTraceIdentifier,
                "Replacing TraceIdentifier ({TraceIdentifier}) by correlation ID");

        public static void CorrelatedRequestBegin(this ILogger logger) =>
            _correlatedRequestBegin(logger, null!);

        public static void CorrelatedRequestEnd(this ILogger logger) =>
            _correlatedRequestEnd(logger, null!);

        public static void CorrelationIdMissing(this ILogger logger, string traceIdentifier) =>
            _correlationIdMissing(logger, traceIdentifier, null!);

        public static void CorrelationIdReceived(this ILogger logger, CorrelationId correlationId) =>
            _correlationIdReceived(logger, correlationId, null!);

        public static void WritingCorrelatedResponse(this ILogger logger) =>
            _writingCorrelatedResponse(logger, null!);

        public static void ReplacingTraceIdentifier(this ILogger logger, string traceIdentifier) =>
            _replacingTraceIdentifier(logger, traceIdentifier, null!);
    }
}
