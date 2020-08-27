## Configuration

By default, Correlator is configured following way:

- Accepting Correlation ID from following headers:
  - `Request-Id`
  - `X-Correlation-Id`
  - `X-Request-Id`
- By default, when there are multiple correlation headers sent, it is not guaranteed which one is going to be read
- When request correlation is missing or has empty value, new correlation ID is generated in form of GUID
- Correlation ID is forwarded to subsequent requests via `X-Correlation-Id` (when using `CorrelatorHttpMessageHandler`)
- Correlation ID is not set to HTTP response headers
- Correlation ID replaces [`HttpContext.TraceIdentifier`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.traceidentifier)
- Correlation ID is not added to logger scope

To adjust setting, use `AddDefaultCorrelator` or `AddCorrelator` overload:

```csharp
services.AddDefaultCorrelator(
    correlatorOptions =>
    {
        // disable correlation ID factory
        // - correlation ID won't be generated
        // - you are getting correlation ID only from incoming request
        correlatorOptions.Factory = null;

        // expect correlation ID in "X-CID" header only
        correlatorOptions.ReadFrom.Clear();
        correlatorOptions.ReadFrom.Add("X-CID");

        // return correlation ID in response, use custom header
        correlatorOptions.Emit = PropagationSettings.PropagateAs("X-RID");

        // forward via message handler
        // - when correlation ID received, using same header name ("X-CID")
        // - when correlation ID not received and generated, sending it as "X-Correlation-Id"
        correlatorOptions.Forward = PropagationSettings.KeepIncomingHeaderName();

        // don't write correlation ID to HttpContext.TraceIdentifier
        ReplaceTraceIdentifier = false,

        // create logging scope with given key
        LoggingScope = LoggingScopeSettings.IncludeLoggingScope("correlation"),
    });
```

### Correlation ID factory

Property `CorrelatorOptions.Factory`, of type `Func<HttpContext, CorrelationId>`.

If set to `null`, generating of correlation ID is disabled - you now rely on caller that correlation ID is always
present in request.

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

First header satisfying match is returned.

```csharp
// add to defaults
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

Notice that `KeepIncomingHeaderName(string = null)` has argument. When Correlation ID is not received, there's
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

```csharp
// no logging scope
correlatorOptions.LoggingScope = LoggingScopeSettings.NoScope;

// logging scope with: correlation = <Correlation ID>
correlatorOptions.LoggingScope = LoggingScopeSettings.IncludeLoggingScope("correlation");
```
