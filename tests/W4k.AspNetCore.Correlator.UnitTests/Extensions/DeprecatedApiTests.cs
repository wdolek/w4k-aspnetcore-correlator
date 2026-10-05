using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Context;
using W4k.AspNetCore.Correlator.Http;
using W4k.AspNetCore.Correlator.Options;
using W4k.AspNetCore.Correlator.Validation;

// Deprecated namespaces are exercised on purpose, this is the only place where CS0618 is expected.
// The file is declared within the deprecated namespace so that its extension methods win over
// the current ones during overload resolution. Delete together with the deprecated members.
#pragma warning disable CS0618

// ReSharper disable once CheckNamespace
namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

public class DeprecatedApiTests
{
    [Test]
    public async Task AddCorrelator_WhenCalled_ExpectSameRegistrationsAsCurrentApi()
    {
        // arrange
        var deprecated = new ServiceCollection();
        var current = new ServiceCollection();

        // act
        deprecated.AddCorrelator();
        W4k.AspNetCore.Correlator.ServiceCollectionExtensions.AddCorrelator(current);

        // assert
        await Assert.That(Describe(deprecated)).IsEquivalentTo(Describe(current));
    }

    [Test]
    public async Task AddCorrelator_WhenConfigureOptionsPassed_ExpectOptionsConfigured()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.AddCorrelator(options => options.ReplaceTraceIdentifier = true);

        // assert
        var resolved = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<CorrelatorOptions>>()
            .Value;

        await Assert.That(resolved.ReplaceTraceIdentifier).IsTrue();
    }

    [Test]
    public async Task AddDefaultCorrelator_WhenCalled_ExpectSameRegistrationsAsCurrentApi()
    {
        // arrange
        var deprecated = new ServiceCollection();
        var current = new ServiceCollection();

        // act
        deprecated.AddDefaultCorrelator();
        W4k.AspNetCore.Correlator.ServiceCollectionExtensions.AddDefaultCorrelator(current);

        // assert
        await Assert.That(Describe(deprecated)).IsEquivalentTo(Describe(current));
        await Assert.That(ImplementationOf<ICorrelationContextFactory>(deprecated)).IsEqualTo(typeof(CorrelationContextFactory));
        await Assert.That(ImplementationOf<ICorrelationEmitter>(deprecated)).IsEqualTo(typeof(CorrelationEmitter));
    }

    [Test]
    public async Task AddDefaultCorrelator_WhenConfigureOptionsPassed_ExpectOptionsConfigured()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.AddDefaultCorrelator(options => options.ReplaceTraceIdentifier = true);

        // assert
        var resolved = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<CorrelatorOptions>>()
            .Value;

        await Assert.That(resolved.ReplaceTraceIdentifier).IsTrue();
    }

    [Test]
    public async Task WithCorrelationContextFactory_WhenCalled_ExpectFactoryRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services
            .AddCorrelator()
            .WithCorrelationContextFactory<CorrelationContextFactory>();

        // assert
        await Assert.That(ImplementationOf<ICorrelationContextFactory>(services)).IsEqualTo(typeof(CorrelationContextFactory));
    }

    [Test]
    public async Task WithDefaultCorrelationContextFactory_WhenCalled_ExpectFactoryRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services
            .AddCorrelator()
            .WithDefaultCorrelationContextFactory();

        // assert
        await Assert.That(ImplementationOf<ICorrelationContextFactory>(services)).IsEqualTo(typeof(CorrelationContextFactory));
    }

    [Test]
    public async Task WithCorrelationEmitter_WhenCalled_ExpectEmitterRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services
            .AddCorrelator()
            .WithCorrelationEmitter<CorrelationEmitter>();

        // assert
        await Assert.That(ImplementationOf<ICorrelationEmitter>(services)).IsEqualTo(typeof(CorrelationEmitter));
    }

    [Test]
    public async Task WithDefaultCorrelationEmitter_WhenCalled_ExpectEmitterRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services
            .AddCorrelator()
            .WithDefaultCorrelationEmitter();

        // assert
        await Assert.That(ImplementationOf<ICorrelationEmitter>(services)).IsEqualTo(typeof(CorrelationEmitter));
    }

    [Test]
    public async Task WithValidator_WhenCalled_ExpectValidatorRegistered()
    {
        // arrange
        var validator = new CorrelationValueLengthValidator(16);
        var services = new ServiceCollection();

        // act
        services
            .AddCorrelator()
            .WithValidator(validator);

        // assert
        var registered = services
            .Any(sd => sd.ServiceType == typeof(ICorrelationValidator) && ReferenceEquals(sd.ImplementationInstance, validator));

        await Assert.That(registered).IsTrue();
    }

    [Test]
    public async Task WithCorrelation_WhenCalled_ExpectSameRegistrationsAsCurrentApi()
    {
        // arrange
        var deprecated = new ServiceCollection();
        var current = new ServiceCollection();

        // act
        deprecated
            .AddHttpClient("client")
            .WithCorrelation();

        W4k.AspNetCore.Correlator.HttpClientBuilderExtensions.WithCorrelation(current.AddHttpClient("client"));

        // assert
        await Assert.That(Describe(deprecated)).IsEquivalentTo(Describe(current));
    }

    [Test]
    public async Task WithCorrelation_WhenPropagationSettingsPassed_ExpectSameRegistrationsAsCurrentApi()
    {
        // arrange
        var settings = PropagationSettings.PropagateAs("X-Correlation-Id");
        var deprecated = new ServiceCollection();
        var current = new ServiceCollection();

        // act
        deprecated
            .AddHttpClient("client")
            .WithCorrelation(settings);

        W4k.AspNetCore.Correlator.HttpClientBuilderExtensions.WithCorrelation(current.AddHttpClient("client"), settings);

        // assert
        await Assert.That(Describe(deprecated)).IsEquivalentTo(Describe(current));
    }

    [Test]
    public async Task UseCorrelator_WhenCalled_ExpectCorrelatorMiddlewareInPipeline()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDefaultCorrelator();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);
        var observed = string.Empty;

        // act
        app.UseCorrelator();
        app.Run(context =>
        {
            observed = context.RequestServices
                .GetRequiredService<ICorrelationContextAccessor>()
                .CorrelationContext.CorrelationId.Value;

            return Task.CompletedTask;
        });

        await app.Build().Invoke(new DefaultHttpContext { RequestServices = serviceProvider });

        // assert
        await Assert.That(observed).IsNotEmpty();
    }

    private static Type? ImplementationOf<TService>(IServiceCollection services) =>
        services
            .FirstOrDefault(sd => sd.ServiceType == typeof(TService))?
            .ImplementationType;

    private static IEnumerable<string> Describe(IServiceCollection services) =>
        services.Select(sd => $"{sd.Lifetime} {sd.ServiceType.FullName} -> {sd.ImplementationType?.FullName ?? "<factory>"}");
}

#pragma warning restore CS0618
