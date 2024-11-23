## DI Registration

Apart of registering default implementations of `ICorrelationContextFactory` and `ICorrelationEmitter`, you can
also provide own implementation.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCorrelator()
    .WithCorrelationContextFactory<MyLittleCorrelationContextFactory>()
    .WithCorrelationEmitter<MyLittleCorrelationEmitter>();
```

### Simple registration

```csharp
builder.Services.AddDefaultCorrelator();

// or
builder.Services.AddDefaultCorrelator(options => { /* */ });
```

### Custom registration

```csharp
ICorrelatorBuilder correlationBuilder = builder.Services.AddCorrelator();

// or
ICorrelatorBuilder correlationBuilder = builder.Services.AddCorrelator(options => { /* */ });
```

... and then either of:
```csharp
// correlation context factory
correlationBuilder.WithDefaultCorrelationContextFactory(); // default implementation
correlationBuilder.WithCorrelationContextFactory<T>(); // type registration
correlationBuilder.WithCorrelationContextFactory(T factory); // instance registration

// correlation emitter
correlationBuilder.WithDefaultCorrelationEmitter(); // default implementation
correlationBuilder.WithCorrelationEmitter<T>(); // type registration
correlationBuilder.WithCorrelationEmitter(T emitter); // instance registration
```

Both `ICorrelationContextFactory` and `ICorrelationEmitter` are registered as singletons.

Note that only one component can be registered. Any attempt to register another implementation of same interface will
end up with exception.

### Correlation value validator

Optionally, you can register correlation validator like this (by default, no validator is registered):

```csharp
correlationBuilder.WithValidator(new CorrelationValueLengthValidator(64));
```

`ICorrelationValidator` is registered as singleton.

Note that you can register only one validator! If you need to use multiple validators, implement composite validator
yourself.
