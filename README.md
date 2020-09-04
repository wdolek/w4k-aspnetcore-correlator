# W4k.AspNetCore.Correlator

![W4k.AspNetCore.Correlator Build](https://github.com/wdolek/w4k-aspnetcore-correlator/workflows/Build%20and%20test/badge.svg) [![NuGet Badge](https://buildstats.info/nuget/W4k.AspNetCore.Correlator)](https://www.nuget.org/packages/W4k.AspNetCore.Correlator/)

Correlator helps you with handling correlation ID (also "request ID"): reading, generating new one and forwarding
to subsequent requests.

Correlation ID is sent within HTTP headers. If header is not set, Correlator will happily generate new one for you.

Apart of accepting or generating correlation ID, it is also possible to return correlation ID back to caller,
so in case when correlation ID is generated, caller is aware of that value.

To forward correlation ID to subsequent request, it is necessary to use designated HTTP message handler, see
examples below.

## W3 Trace Context and .NET Core 3.x

Please be aware that [Trace Context](https://www.w3.org/TR/trace-context/) is **not supported** out of the box,
Correlator helps you with simple non-standard headers.

Note that with .NET Core 3.x, distributed tracing and trace context is built in. You can get more insights from article:
[Improvements in .NET Core 3.0 for troubleshooting and monitoring distributed apps](https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/).

## Basic usage

### Startup class

```csharp
public class MyLittleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultCorrelator();
    }

    public void Configure(IApplicationBuilder app)
    {
        // register as first middleware
        // (or as soon as possible to benefit from having correlation ID)
        app.UseCorrelator();

        app.UseMvc();
    }
}
```

### Accessing correlation ID

Correlation ID of current request is available via `ICorrelationContextAccessor.CorrelationContext`:

```csharp
using W4k.AspNetCore.Correlator;
using W4k.AspNetCore.Correlator.Context;

public class MyLittleService
{
    private readonly ICorrelationContextAccessor _contextAccessor;

    public MyLittleService(ICorrelationContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public async Task DoMagicalStuff()
    {
        CorrelationId correlationId = _contextAccessor.CorrelationContext.CorrelationId;

        // ...
    }
}
```

## Forwarding correlation ID

In order to pass correlation ID to subsequent requests, additional HTTP message handler has to be registered.

Add `CorrelatorHttpMessageHandler` to HTTP client's message handler pipeline like this:

```csharp
public class MyLittleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // named HTTP client
        services
            .AddHttpClient("DummyClient")
            .WithCorrelation();

        // typed HTTP client
        services
            .AddHttpClient<FooClient>()
            .WithCorrelation();

        // registering HTTP message handler manually
        services
            .AddHttpClient("FizzClient")
            .AddHttpMessageHandler<CorrelatorHttpMessageHandler>();

        // registering HTTP client with custom settings
        // (global options - CorrelatorOptions.Forward - won't be used)
        services
            .AddHttpClient<LegacyClient>()
            .WithCorrelation(PropagationSettings.PropagateAs("X-Legacy-Correlation-Id"));
    }

    // ...
}
```

See "[Configure the HttpMessageHandler](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1#configure-the-httpmessagehandler)" for more details about usage of HTTP message handler.

## Validation of correlation ID

It is possible to validate correlation ID value and prevent invalid value to spread. By default, validation is
turned off. In order to turn validation on, implementation of `ICorrelationValidator` musth be registered.

Correlator is shipped with lightweight validator, `CorrelationValueLengthValidator`, which decides whether received
value is valid simply based on its length.

To enable validation, register instance of validator following way:
```
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddDefaultCorrelator()
        .WithValidator(new CorrelationValueLengthValidator(128));
}
```

Validator is registered as singleton.

## Documentation

- [Configuration](docs/configuration.md): detailed description of configuration
- [Dependency injection](docs/registration.md): additional ways how to register Correlator into DI
- [Component description](docs/components.md)
- [Alternative packages and further reading](docs/alternatives.md)
