using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Options;

public class CorrelatorOptionsValidatorTests
{
    [Test]
    public async Task Validate_WhenNoHeaderNamesConfigured_ExpectFailure()
    {
        var options = new CorrelatorOptions();
        options.ReadFrom.Clear();

        var result = new CorrelatorOptionsValidator().Validate(name: null, options);

        await Assert.That(result.Failed).IsTrue();
        await Assert.That(result.FailureMessage).Contains(nameof(CorrelatorOptions.ReadFrom));
    }

    [Test]
    public async Task Validate_WhenInvalidHeaderNameConfigured_ExpectFailureWithInvalidHeaderNames()
    {
        var options = new CorrelatorOptions();
        options.ReadFrom.Clear();
        options.ReadFrom.Add("X CID");
        options.ReadFrom.Add("X-Correlation-Id ");

        var result = new CorrelatorOptionsValidator().Validate(name: null, options);

        await Assert.That(result.Failed).IsTrue();
        await Assert.That(result.FailureMessage).Contains("X CID");
        await Assert.That(result.FailureMessage).Contains("X-Correlation-Id ");
    }

    [Test]
    public async Task Validate_WhenValidHeaderNamesConfigured_ExpectSuccess()
    {
        var result = new CorrelatorOptionsValidator().Validate(name: null, new CorrelatorOptions());

        await Assert.That(result.Succeeded).IsTrue();
    }
}
