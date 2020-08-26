## Correlation Context Factory

Context factory, implementing `ICorrelationContextFactory`, is responsible for reading correlation out
of request's [`HttpContext`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext).

```csharp
CorrelationContext CreateContext(HttpContext httpContext)
```

### Correlation Context

Correlation Context Factory returns base type `CorrelationContext`, which is then implemented by following types:

- `EmptyCorrelationContext`: No correlation received nor generated - we don't know correlation
- `GeneratedCorrelationContext`: No correlation received, but generated using factory method (see options)
- `RequestCorrelationContext`: Correlation ID received

**Important**: When providing own implementation, make sure it does not throw any exception!

## Correlation Emitter

Correlation emitter, implementing `ICorrelationEmitter`, is responsible for forwarding Correlation ID
to subsequent requests. Setting response headers is done by registering emitter using
[`HttpResponse.OnStarting(Func<Task>)`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpresponse.onstarting#Microsoft_AspNetCore_Http_HttpResponse_OnStarting_System_Func_System_Threading_Tasks_Task__);

```csharp
Task Emit(HttpContext httpContext, CorrelationContext correlationContext)
```

**Important**: When providing own implementation, make sure it does not throw any exception!

## Correlator HTTP Message Handler

Correlator HTTP Message Handler extends [`DelegatingHandler`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler).

```csharp
Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
```
