# W4k.AspNetCore.Correlator

Correlator helps you with handling correlation ID (known also as request ID):
reading, generating new one and forwarding to subsequent requests.

Correlation ID is sent within HTTP headers. If header is not set,
Correlator will happily generate new one for you (ASP.NET itself generates
`TraceIdentifier` - and it is still possible to stick with its value).

Apart of accepting or generating correlation ID, it is also possible to emit correlation ID
back to caller within HTTP response headers.

To forward correlation ID to subsequent request, it is necessary to use
designated HTTP message handler, see examples below.

## W3: Trace Context

Please be aware that [Trace Context](https://www.w3.org/TR/trace-context/) is not supported,
Correlator helps you with older systems using non-standard headers.
See links below for more alternatives.

## Basic usage

Correlator consists of two components. Middleware which reads (generates if missing)
correlation ID, and HTTP message handler which adds your correlation to outbound requests.

Following examples demonstrate how to wire everything up.

### Startup class

Register components and add `CorrelatorMiddleware` to application pipeline:

```csharp
public class MyLittleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelator();

        // other stuff
        // ...
    }

    public void Configure(IApplicationBuilder app)
    {
        // register as first middleware
        app.UseCorrelator();

        // other stuff
        // ...

        app.UseMvc();
    }
}
```

### Forwarding correlation ID

Add `CorrelatorHttpMessageHandler` to HTTP client's message handler pipeline like this:

```csharp
services
    .AddHttpClient()
    .WithCorrelation();
```

### Accessing correlation ID within application

Correlation ID of current request is always available from `HttpContext.TraceIdentifier`.
Even if you disable generating of correlation ID, ASP.NET itself already sets its value.

To be able to access `HttpContext` from your component, you need to inject `IHttpContextAccessor`.

```csharp
using Microsoft.AspNetCore.Http;

public class MyLittleService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public MyLittleService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    public async Task DoMagicalStuff()
    {
        HttpContext context = _contextAccessor.HttpContext;
        string correlationId = context?.TraceIdentifier;
        
        // We found a Correlation ID, of Candy Mountain. Candy Mountain, Charlie.
        // ...
    }
}
```

## Configuration

By default, correlator behaves following way:

- Headers used for accepting correlation ID are: `Request-Id`, `X-Correlation-Id` and `X-Request-Id`
- If correlation ID header is missing or has empty value, new correlation ID is generated in form of GUID
- Correlation ID is forwarded to subsequent requests using `CorrelatorHttpMessageHandler` (as `X-Correlation-ID`)
- Correlation ID is not set to HTTP response headers

To adjust setting, use `AddCorrelator` overload:

```csharp
services.AddCorrelator(
    correlatorOptions =>
    {
        // disable correlation ID factory, stick with ASP.NET value
        correlatorOptions.Factory = null;

        // read only custom header
        correlatorOptions.ReadFrom.Clear();
        correlatorOptions.ReadFrom.Add("X-CID");

        // include correlation ID in response
        correlatorOptions.Emit = PropagationSettings.PropagateAs("X-RID");

        // forward via message handler, using same header name ("X-CID")
        correlatorOptions.Forward = PropagationSettings.KeepIncomingHeaderName;
    });
```

### Correlation ID factory (`Factory`)

Property `CorrelatorOptions.Factory`, of type `Func<CorrelationId>`.

If set to `null` then generating of correlation ID is disabled and ASP.NET value is used instead.
For more details look up for `HttpContext.TraceIdentifier`.

```csharp
// default
correlatorOptions.Factory = () => CorrelationId.NewCorrelationId();

// custom (don't even try this, this is just demonstration)
correlatorOptions.Factory = () => CorrelationId.FromString("hello world!").Value;

// disable (stick with ASP.NET TraceIdentifier)
correlatorOptions.Factory = null;
```

### Read from headers (`ReadFrom`)

Property `CorrelatorOptions.ReadFrom`, of type `ICollection<string>`.

Note that order matters - first header satisfying match is returned.

```csharp
// add to defaults
correlatorOptions.Add("X-Yet-Another-Request-ID");

// read only from given header
correlatorOptions.Clear();
correlatorOptions.Add("X-This-Is-Only-Possible-Correlation-ID-Now");
```

### Correlation ID propagation (`Emit`, `Forward`)

There are two directions to propagate correlation ID:

1. `CorrelatorOptions.Emit`: Include correlation ID of request in response headers
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
correlatorOptions.Emit = PropagationSettings.KeepIncomingHeaderName;
correlatorOptions.Forward = PropagationSettings.KeepIncomingHeaderName;
```

#### Predefined header name

Correlation ID is propagated with predefined header name.

```csharp
// if correlation was read from 'X-My-Custom-Correlation-Id', it is exposed as 'X-Correlation-Id'
correlatorOptions.Emit = PropagationSettings.PropagateAs("X-Correlation-Id");
correlatorOptions.Forward = PropagationSettings.PropagateAs("X-Correlation-Id");
```

## Alternative packages

- [CorrelationId](https://www.nuget.org/packages/CorrelationId/) by Steve Gordon.
- [Correlate](https://www.nuget.org/packages/Correlate.AspNetCore/) by Martijn Bodeman.

## Advanced tracing

This package is designed to solve simple scenario with reading and writing correlation ID from/to
HTTP headers. If you are looking for more advanced tracing, consider checking:
[OpenTracing](https://opentracing.io/) /
[Zipkin](https://zipkin.io/) /
[Jaeger](https://www.jaegertracing.io/).
