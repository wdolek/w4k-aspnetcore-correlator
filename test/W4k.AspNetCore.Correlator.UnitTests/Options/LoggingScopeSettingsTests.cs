using System;
using System.Collections.Generic;
using Xunit;

namespace W4k.AspNetCore.Correlator.Options;

public class LoggingScopeSettingsTests
{
    [Fact]
    public void Factory_IncludeScope_ExpectIncludeLoggingScopeSettings()
    {
        var settings = LoggingScopeSettings.IncludeLoggingScope();

        Assert.True(settings.IncludeScope);
        Assert.Equal("Correlation", settings.CorrelationKey);
    }

    [Fact]
    public void Factory_NoScope_ExpectNoScopeSettings()
    {
        var settings = LoggingScopeSettings.NoScope;

        Assert.False(settings.IncludeScope);
    }

    [Theory]
    [MemberData(nameof(GenerateDefaultLoggingScopeSettings))]
    public void Ctor_InstantiatedUsingDefault_ExpectNoPropagation(LoggingScopeSettings loggingScope)
    {
        Assert.False(loggingScope.IncludeScope);
        Assert.Equal("Correlation", loggingScope.CorrelationKey);
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    public void IncludeLoggingScope_WhenEmptyInput_Throws(string input, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => LoggingScopeSettings.IncludeLoggingScope(input));
    }

    public static IEnumerable<object[]> GenerateDefaultLoggingScopeSettings()
    {
        yield return new object[] { new LoggingScopeSettings() };
        yield return new object[] { default(LoggingScopeSettings) };
    }
}
