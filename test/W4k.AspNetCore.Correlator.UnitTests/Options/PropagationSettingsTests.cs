using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using W4k.AspNetCore.Correlator.Http;

namespace W4k.AspNetCore.Correlator.Options;

public class PropagationSettingsTests
{
    [Test]
    public async Task Factory_NoPropagation_ExpectNoPropagationEnum()
    {
        var settings = PropagationSettings.NoPropagation;

        await Assert.That(settings.Settings).IsEqualTo(HeaderPropagation.NoPropagation);
    }

    [Test]
    public async Task Factory_KeepIncoming_ExpectKeepIncomingSettings()
    {
        var settings = PropagationSettings.KeepIncomingHeaderName();

        await Assert.That(settings.Settings).IsEqualTo(HeaderPropagation.KeepIncomingHeaderName);
        await Assert.That(settings.HeaderName).IsEqualTo(HttpHeaders.CorrelationId);
    }

    [Test]
    public async Task Factory_Predefined_ExpectPredefinedSettings()
    {
        var settings = PropagationSettings.PropagateAs("X-Test-Correlation-Id");

        await Assert.That(settings.Settings).IsEqualTo(HeaderPropagation.UsePredefinedHeaderName);
        await Assert.That(settings.HeaderName).IsEqualTo("X-Test-Correlation-Id");
    }

    [Test]
    [MethodDataSource(nameof(GenerateDefaultPropagationSettings))]
    public async Task Ctor_InstantiatedUsingDefault_ExpectNoPropagation(PropagationSettings propagation)
    {
        await Assert.That(propagation.HeaderName).IsEqualTo(HttpHeaders.CorrelationId);
        await Assert.That(propagation.Settings).IsEqualTo(HeaderPropagation.NoPropagation);
    }

    [Test]
    [Arguments(null, typeof(ArgumentNullException))]
    [Arguments("", typeof(ArgumentException))]
    public void PropagateAs_WhenEmptyInput_Throws(string? input, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => PropagationSettings.PropagateAs(input!));
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task KeepIncomingHeader_WhenEmpty_ExpectDefault(string? input)
    {
        var settings = PropagationSettings.KeepIncomingHeaderName(input);

        await Assert.That(settings.Settings).IsEqualTo(HeaderPropagation.KeepIncomingHeaderName);
        await Assert.That(settings.HeaderName).IsEqualTo(HttpHeaders.CorrelationId);
    }

    public static IEnumerable<object[]> GenerateDefaultPropagationSettings()
    {
        yield return new object[] { new PropagationSettings() };
        yield return new object[] { default(PropagationSettings) };
    }
}