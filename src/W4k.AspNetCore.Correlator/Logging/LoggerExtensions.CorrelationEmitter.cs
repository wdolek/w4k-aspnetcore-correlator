using System;
using Microsoft.Extensions.Logging;

namespace W4k.AspNetCore.Correlator.Logging;

internal static partial class LoggerExtensions
{
    private static readonly EventId WritingCorrelatedResponseEvent =
        new EventId(301, nameof(LoggerExtensions.WritingCorrelatedResponse));

    private static readonly Action<ILogger, Exception> LogWritingCorrelatedResponse =
        LoggerMessage.Define(
            LogLevel.Debug,
            WritingCorrelatedResponseEvent,
            "Writing correlation ID to response headers");

    public static void WritingCorrelatedResponse(this ILogger logger) =>
        LogWritingCorrelatedResponse(logger, null!);
}