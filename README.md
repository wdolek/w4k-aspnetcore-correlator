# W4k.AspNetCore.Correlator

Correlator helps you with handling correlation ID (known also as request ID):
reading, generating new one and forwarding to subsequent requests.

Correlation ID is sent within HTTP headers. If header is not set,
Correlator will happily generate new one for you (well, ASP.NET itself generates
`TraceIdentifier` - and it is still possible to stick with its value).

Apart of accepting or generating correlation ID, it is also possible to emit correlation ID
back to caller within HTTP response headers.

To forward correlation ID to subsequent request, it is necessary to use
designated HTTP message handler.

## Basic usage

To make all those wonders happen, `CorrelatorMiddleware` must be used.

### Startup class example
```csharp
public class MyLittleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelator();
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

Add `CorrelatorHttpMessageHandler` to pipeline when registering particular implementation of HTTP client.

```csharp
services.AddHttpClient("svc", c =>
{
    // HTTP client configuration
})
.AddHttpMessageHandler<CorrelatorHttpMessageHandler>();
```

### Accessing correlation ID from application

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

- Headers used for accepting correlation ID are: `X-Correlation-ID` and `X-Request-ID`
- If correlation ID header is missing or has empty value, new correlation ID is generated in form of GUID
- Correlation ID is forwarded to subsequent requests using `CorrelatorHttpMessageHandler` (as `X-Correlation-ID`)
- Correlation ID is not set to HTTP response headers

To adjust setting, use `Configure<CorrelatorOptions>` method to override defaults.

```csharp
services.Configure<CorrelatorOptions>(
    o =>
    {
        // disable correlation ID factory, stick with ASP.NET value
        o.Factory = null;

        // read only custom header
        o.ReadFrom.Clear();
        o.ReadFrom.Add("X-CID");

        // include correlation ID in response
        o.Emit = PropagationSettings.PropagateAs("X-RID");

        // forward via message handler, using same header name ("X-CID")
        o.Forward = PropagationSettings.KeepIncomingHeaderName;
    });
```

### Read from headers

Property `CorrelatorOptions.ReadFrom`, of type `ICollection<string>`.

By default, Correlator tries to get value from headers: `X-Correlation-ID` and `X-Request-ID`.
Header must be present and contain non-empty value.

Note that order matters - first header satisfying match is returned.

```csharp
// add to defaults
o.Add("X-Yet-Another-Request-ID");

// read only from given header
o.Clear();
o.Add("X-This-Is-Only-Possible-Correlation-ID-Now");
```

### Correlation ID factory

Property `CorrelatorOptions.Factory`, of type `Func<CorrelationId>`.

If set to `null` then generating of correlation ID is disabled and ASP.NET value is used instead.
For more details look up for `HttpContext.TraceIdentifier`.

```csharp
// default
o.Factory = () => CorrelationId.NewCorrelationId();

// custom (don't even try this, this is just demonstration)
o.Factory = () => CorrelationId.FromString("hello world!").Value;

// disable (stick with ASP.NET TraceIdentifier)
o.Factory = null;
```

### Correlation ID propagation

There are two directions to propagate correlation ID:

1. `CorrelatorOptions.Emit`: Include correlation ID of request in response headers
2. `CorrelatorOptions.Forward`: Forward correlation ID to subsequent requests using `CorrelatorHttpMessageHandler`

#### No propagation

Correlation ID is not set.

```csharp
// don't expose correlation ID in HTTP response
o.Emit = PropagationSettings.NoPropagation;

// don't forward correlation ID to subsequent requests
o.Forward = PropagationSettings.NoPropagation;
```

#### Use incoming header name

Correlation ID is propagated with same header name as it was read from.

```csharp
// if correlation was taken from 'X-My-Custom-Correlation-ID', it is exposed with same header
o.Emit = PropagationSettings.KeepIncomingHeaderName;
```

#### Predefined header name

Correlation ID is propagated with predefined header name.

```csharp
// if correlation was read from 'X-My-Custom-Correlation-ID', it is exposed as 'X-Correlation-ID'
o.Emit = PropagationSettings.PropagateAs("X-Correlation-ID");
```

## Alternatives

- [CorrelationId](https://www.nuget.org/packages/CorrelationId/) by Steve Gordon.

## Advanced tracing

This package is designed to solve simple scenario with reading and writing correlation ID from/to
HTTP headers. If you are looking for more advanced tracing, consider checking:
[OpenTracing](https://opentracing.io/) /
[Zipkin](https://zipkin.io/) /
[Jaeger](https://www.jaegertracing.io/).
