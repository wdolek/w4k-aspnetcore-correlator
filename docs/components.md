## Correlation Context Factory

Context factory, implementing `ICorrelationContextFactory`, is responsible for reading correlation out
of request's [`HttpContext`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext).

```csharp
CorrelationContext CreateContext(HttpContext httpContext)
```

### Correlation Context

Correlation Context Factory returns (abstract) `CorrelationContext`, which is then extended by following types:

- `EmptyCorrelationContext`: No correlation received nor generated - we don't know correlation
- `GeneratedCorrelationContext`: No correlation received, but generated using configured factory method
- `RequestCorrelationContext`: Correlation ID received
- `InvalidCorrelationContext`: Correlation received, but invalid

## Correlation Emitter

Correlation emitter, implementing `ICorrelationEmitter`, is responsible for forwarding Correlation ID
to subsequent requests.

```csharp
Task Emit(HttpContext httpContext, CorrelationContext correlationContext)
```

## Correlation validator

Correlation validator, implementing `ICorrelationValidator`, (if registered) checks whether incoming raw
value of correlation header is valid.

If value is invalid `InvalidCorrelationContext` is created by context factory instead of `RequestCorrelationContext`.

```csharp
ValidationResult Validate(string? value);
```

## Correlator HTTP Message Handler

Correlator HTTP Message Handler extends [`DelegatingHandler`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler).

```csharp
Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
```
