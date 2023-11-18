using System;
using System.Collections.Generic;
using W4k.AspNetCore.Correlator.Http;
using Xunit;

namespace W4k.AspNetCore.Correlator.Options;

public class PropagationSettingsTests
{
    [Fact]
    public void Factory_NoPropagation_ExpectNoPropagationEnum()
    {
        var settings = PropagationSettings.NoPropagation;

        Assert.Equal(HeaderPropagation.NoPropagation, settings.Settings);
    }

    [Fact]
    public void Factory_KeepIncoming_ExpectKeepIncomingSettings()
    {
        var settings = PropagationSettings.KeepIncomingHeaderName();

        Assert.Equal(HeaderPropagation.KeepIncomingHeaderName, settings.Settings);
        Assert.Equal(HttpHeaders.CorrelationId, settings.HeaderName);
    }

    [Fact]
    public void Factory_Predefined_ExpectPredefinedSettings()
    {
        var settings = PropagationSettings.PropagateAs("X-Test-Correlation-Id");

        Assert.Equal(HeaderPropagation.UsePredefinedHeaderName, settings.Settings);
        Assert.Equal("X-Test-Correlation-Id", settings.HeaderName);
    }

    [Theory]
    [MemberData(nameof(GenerateDefaultPropagationSettings))]
    public void Ctor_InstantiatedUsingDefault_ExpectNoPropagation(PropagationSettings propagation)
    {
        Assert.Equal(HttpHeaders.CorrelationId, propagation.HeaderName);
        Assert.Equal(HeaderPropagation.NoPropagation, propagation.Settings);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PropagateAs_WhenEmptyInput_Throws(string input)
    {
        Assert.Throws<ArgumentNullException>(() => PropagationSettings.PropagateAs(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void KeepIncomingHeader_WhenEmpty_ExpectDefault(string input)
    {
        var settings = PropagationSettings.KeepIncomingHeaderName(input);

        Assert.Equal(HeaderPropagation.KeepIncomingHeaderName, settings.Settings);
        Assert.Equal(HttpHeaders.CorrelationId, settings.HeaderName);
    }

    public static IEnumerable<object[]> GenerateDefaultPropagationSettings()
    {
        yield return new object[] { new PropagationSettings() };
        yield return new object[] { default(PropagationSettings) };
    }
}