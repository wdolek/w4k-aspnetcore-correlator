## Configuration

By _default_, Correlator is configured following way:

- **Accepting** Correlation ID from following headers (in order):
  - `X-Correlation-Id`
  - `X-Request-Id`
  - `Request-Id` (set by ASP.NET when sending request with `HttpClient`)
- Correlation ID **is forwarded** to subsequent requests as `X-Correlation-Id` (when using `CorrelatorHttpMessageHandler`)
- Correlation ID **is not set** to HTTP response headers
- Correlation ID **does not replace** [`HttpContext.TraceIdentifier`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.traceidentifier)
- Correlation ID **is not added** to logger scope
- Correlation **logging scope is** (after enabling) `"Correlation"`

### Adjusting default configuration

To adjust setting, use `AddDefaultCorrelator` or `AddCorrelator` overload:

```csharp
services.AddDefaultCorrelator(
    correlatorOptions =>
    {
        // disable correlation ID factory
        // - correlation ID won't be generated
        // --> if correlation ID not received, empty value is used
        correlatorOptions.Factory = null;

        // expect correlation ID in "X-CID" header only
        // (clearing default headers first, then adding single entry)
        correlatorOptions.ReadFrom.Clear();
        correlatorOptions.ReadFrom.Add("X-CID");

        // return correlation ID in response, use custom header
        correlatorOptions.Emit = PropagationSettings.PropagateAs("X-RID");

        // forward via message handler
        // - when correlation ID received, using same header name ("X-CID")
        // - when correlation ID not received and generated, sending it with default header
        correlatorOptions.Forward = PropagationSettings.KeepIncomingHeaderName();

        // replace `HttpContext.TraceIdentifier`
        ReplaceTraceIdentifier = true,

        // create logging scope with default key
        LoggingScope = LoggingScopeSettings.IncludeLoggingScope(),
    });
```

### Correlation ID factory (function)

Property `CorrelatorOptions.Factory`, of type `Func<HttpContext, CorrelationId>`.

If set to `null`, generating of correlation ID is disabled.

```csharp
// default
correlatorOptions.Factory =
    (_) => CorrelationId.FromString(Guid.NewGuid().ToString("D"));

// custom (don't try this at home!)
correlatorOptions.Factory =
    (_) => CorrelationId.FromString("hello world!");

// disable generating
correlatorOptions.Factory = null;
```

### Read from headers

Property `CorrelatorOptions.ReadFrom`, of type `ICollection<string>`.

First header satisfying match is read. Collection must not be empty.

```csharp
// add after default headers
correlatorOptions.Add("X-Yet-Another-Request-ID");

// read only from given header
correlatorOptions.Clear();
correlatorOptions.Add("X-This-Is-Only-Possible-Correlation-ID-Now");
```

If you are sure about correlation header name, feel free to use just that and avoid unnecessary lookup in request headers.

### Correlation ID propagation

There are two directions to propagate correlation ID:

1. `CorrelatorOptions.Emit`: Return correlation ID within response headers
2. `CorrelatorOptions.Forward`: Forward correlation ID to subsequent requests using `CorrelatorHttpMessageHandler`

#### No propagation

Correlation ID is not set.

```csharp
// don't expose correlation ID in HTTP response
correlatorOptions.Emit = PropagationSettings.NoPropagation;

// don't forward correlation ID to subsequent requests
correlatorOptions.Forward = PropagationSettings.NoPropagation;
```

#### Use incoming header name

Correlation ID is propagated with same header name as it was read from.

```csharp
// if correlation was taken from 'X-My-Custom-Correlation-Id', it is exposed with same header
correlatorOptions.Emit = PropagationSettings.KeepIncomingHeaderName();
correlatorOptions.Forward = PropagationSettings.KeepIncomingHeaderName();
```

Notice argument of `KeepIncomingHeaderName(string)`. When Correlation ID is not received, there's
obviously no way how to determine _incoming header_ - and this is the place where you can help Correlator to know
how to propagate correlation.

Default value of fallback header name is `"X-Correlation-Id"`.

#### Predefined header name

Correlation ID is propagated with predefined header name.

```csharp
// no matter how we got Correlation ID, it is exposed as 'X-Correlation-Id'
correlatorOptions.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
correlatorOptions.Forward = PropagationSettings.PropagateAs("X-Correlation-Id");
```

### Replace Trace Identifier

Property `ReplaceTraceIdentifier`, of type `bool`.

Indicates whether received/generated correlation ID should overwrite ASP.NET Trace Identifier.

### Logging scope

Property `LoggingScope`, of type `LoggingScopeSettings`.

Either:
- `NoScope`: logging scope is not created
- `IncludeLoggingScope(string)`: logging scope is created, correlation ID is saved with provided key

Use scope for structured logging.

```csharp
// no logging scope
correlatorOptions.LoggingScope = LoggingScopeSettings.NoScope;

// logging scope with: Correlation = <Correlation ID>
correlatorOptions.LoggingScope = LoggingScopeSettings.IncludeLoggingScope();

// logging scope with custom scope: CorrelationId = <Correlation ID>
correlatorOptions.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("CorrelationId");
```

## Silencing logger

If you find Correlator to be too chatty, you can always silence logging by:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "W4k.AspNetCore.Correlator": "None"
    }
  }
}
```

... or if you use Serilog:

```
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("W4k.AspNetCore.Correlator", LogEventLevel.Fatal);
```
