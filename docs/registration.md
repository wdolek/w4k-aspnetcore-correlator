## DI Registration

Apart of registering default implementations of `ICorrelationContextFactory` and `ICorrelationEmitter`, you can
also provide own implementation.

```csharp
public class MyLittleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddCorrelator()
            .WithCorrelationContextFactory<MyLittleCorrelationContextFactory>()
            .WithCorrelationEmitter<MyLittleCorrelationEmitter>();
    }

    // ...
}
```

### Simple registration

```
services.AddDefaultCorrelator();

// or
services.AddDefaultCorrelator(options => { /* */ });
```

### Custom registration

```
ICorrelatorBuilder builder = services.AddCorrelator();

// or
ICorrelatorBuilder builder = services.AddCorrelator(options => { /* */ });
```

... and then either of:
```
// correlation context factory
builder.WithDefaultCorrelationContextFactory(); // default implementation
builder.WithCorrelationContextFactory<T>(); // type registration
builder.WithCorrelationContextFactory(T factory); // instance registration

// correlation emitter
builder.WithDefaultCorrelationEmitter(); // default implementation
builder.WithCorrelationEmitter<T>(); // type registration
builder.WithCorrelationEmitter(T emitter); // instance registration
```

Both `ICorrelationContextFactory` and `ICorrelationEmitter` are registered as singletons.

Note that only one component can be registered. Any attempt to register another implementation of same interface will
end up with exception.

### Correlation value validator

Optionally, you can register correlation validator like this (by default, no validator is registered):

```
builder.WithValidator(new CorrelationValueLengthValidator(64));
```

`ICorrelationValidator` is registered as singleton.

Note that you can register only one validator! If you need to use multiple validators, implement composite validator
yourself.
