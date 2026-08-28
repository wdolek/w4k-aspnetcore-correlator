using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Options;

public class LoggingScopeSettingsTests
{
    [Test]
    public async Task Factory_IncludeScope_ExpectIncludeLoggingScopeSettings()
    {
        var settings = LoggingScopeSettings.IncludeLoggingScope();

        await Assert.That(settings.IncludeScope).IsTrue();
        await Assert.That(settings.CorrelationKey).IsEqualTo("Correlation");
    }

    [Test]
    public async Task Factory_NoScope_ExpectNoScopeSettings()
    {
        var settings = LoggingScopeSettings.NoScope;

        await Assert.That(settings.IncludeScope).IsFalse();
    }

    [Test]
    [MethodDataSource(nameof(GenerateDefaultLoggingScopeSettings))]
    public async Task Ctor_InstantiatedUsingDefault_ExpectNoPropagation(LoggingScopeSettings loggingScope)
    {
        await Assert.That(loggingScope.IncludeScope).IsFalse();
        await Assert.That(loggingScope.CorrelationKey).IsEqualTo("Correlation");
    }

    [Test]
    [Arguments(null, typeof(ArgumentNullException))]
    [Arguments("", typeof(ArgumentException))]
    public void IncludeLoggingScope_WhenEmptyInput_Throws(string? input, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => LoggingScopeSettings.IncludeLoggingScope(input!));
    }

    public static IEnumerable<object[]> GenerateDefaultLoggingScopeSettings()
    {
        yield return new object[] { new LoggingScopeSettings() };
        yield return new object[] { default(LoggingScopeSettings) };
    }
}