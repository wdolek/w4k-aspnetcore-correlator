using System;
using Microsoft.Extensions.Logging;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Logging;

internal static partial class LoggerExtensions
{
    private static readonly EventId NoCorrelationHeaderReceivedEvent =
        new EventId(202, nameof(NoCorrelationHeaderReceived));

    private static readonly EventId CorrelationIdReceivedEvent =
        new EventId(203, nameof(CorrelationIdReceived));

    private static readonly EventId NoCorrelationIdFactoryConfiguredEvent =
        new EventId(204, nameof(NoCorrelationIdFactoryConfigured));

    private static readonly EventId GeneratingCorrelationIdEvent =
        new EventId(205, nameof(GeneratingCorrelationId));

    private static readonly EventId InvalidCorrelationValueEvent =
        new EventId(206, nameof(InvalidCorrelationValue));

    private static readonly Action<ILogger, Exception> LogNoCorrelationHeaderReceived =
        LoggerMessage.Define(
            LogLevel.Warning,
            NoCorrelationHeaderReceivedEvent,
            "No correlation HTTP header received");

    private static readonly Action<ILogger, string, string, Exception> LogCorrelationHeaderReceived =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            CorrelationIdReceivedEvent,
            "Correlation ID received, header: {Header}, correlation: {CorrelationId}");

    private static readonly Action<ILogger, Exception> LogNoCorrelationIdFactoryConfigured =
        LoggerMessage.Define(
            LogLevel.Warning,
            NoCorrelationIdFactoryConfiguredEvent,
            $"Correlation ID factory not configured, {nameof(CorrelatorOptions)}.{nameof(CorrelatorOptions.Factory)}");

    private static readonly Action<ILogger, Exception> LogGeneratingCorrelationId =
        LoggerMessage.Define(
            LogLevel.Information,
            GeneratingCorrelationIdEvent,
            "Generating new correlation ID for request");

    private static readonly Action<ILogger, string, string, Exception> LogInvalidCorrelationValue =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            InvalidCorrelationValueEvent,
            "Correlation header ({Header}) value is invalid with: {Reason}");

    public static void NoCorrelationHeaderReceived(this ILogger logger) =>
        LogNoCorrelationHeaderReceived(logger, null!);

    public static void CorrelationIdReceived(this ILogger logger, string header, string value) =>
        LogCorrelationHeaderReceived(logger, header, value, null!);

    public static void NoCorrelationIdFactoryConfigured(this ILogger logger) =>
        LogNoCorrelationIdFactoryConfigured(logger, null!);

    public static void GeneratingCorrelationId(this ILogger logger) =>
        LogGeneratingCorrelationId(logger, null!);

    public static void InvalidCorrelationValue(this ILogger logger, string header, string reason) =>
        LogInvalidCorrelationValue(logger, header, reason, null!);
}