using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using W4k.AspNetCore.Correlator;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(
    options =>
    {
        options.ColorBehavior = LoggerColorBehavior.Disabled;
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
    });

builder.Services.AddDefaultCorrelator(options =>
{
    options.Forward = PropagationSettings.PropagateAs("X-Correlation-Id");
    options.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
    options.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("Correlation");
});

builder.Services
    .AddHttpClient("DummyClient")
    .WithCorrelation();

var app = builder.Build();
var logger = app.Logger;

app.UseCorrelator();

app.MapGet(
    "/",
    ([FromServices] ICorrelationContextAccessor contextAccessor) =>
    {
        logger.LogInformation("Entering hello");

        var correlationId = contextAccessor.CorrelationContext.CorrelationId;
        var result = correlationId.IsEmpty
            ? "<correlation missing>"
            : correlationId;

        logger.LogInformation("Request almost finished, correlation: {CorrelationId}", correlationId);

        return Results.Ok(result);
    });

app.Run();
