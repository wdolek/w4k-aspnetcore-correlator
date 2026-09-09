using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using W4k.AspNetCore.Correlator.Validation;

namespace W4k.AspNetCore.Correlator.Extensions.DependencyInjection;

public class CorrelatorBuilderExtensionsTests
{
    [Test]
    public async Task WithDefaultValidator_WhenCalled_ExpectValidatorRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        var builder = services.AddCorrelator();
        builder.WithDefaultValidator();

        // assert
        await Assert.That(services.Any(svc => svc.ServiceType == typeof(ICorrelationValidator))).IsTrue();
    }

    [Test]
    public void WithDefaultValidator_WhenValidatorAlreadyRegistered_ExpectThrow()
    {
        // arrange
        var services = new ServiceCollection();
        var builder = services
            .AddCorrelator()
            .WithDefaultValidator();

        // act + assert
        Assert.Throws<InvalidOperationException>(() => builder.WithDefaultValidator());
    }
}
