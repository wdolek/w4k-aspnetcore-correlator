using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator;

public class CorrelationIdTests
{
    [Test]
    public async Task StringTypeCast_ExpectInternalValue()
    {
        var correlationId = CorrelationId.FromString("123");
        string value = correlationId;

        await Assert.That(value).IsNotNull();
        await Assert.That(value).IsEqualTo("123");
    }

    [Test]
    [Arguments("123", "123")]
    [Arguments("test", "test")]
    [Arguments("test", "TEST")]
    public async Task Equals_MultipleOverrides_ExpectToBeEqual(string right, string left)
    {
        var c1 = CorrelationId.FromString(right);
        var c2 = CorrelationId.FromString(left);

        await Assert.That(c1.Equals(c2)).IsTrue();
        await Assert.That(c1 == c2).IsTrue();
        await Assert.That(c2).IsEqualTo(c1);
    }

    [Test]
    [Arguments("test_1", "TEST_2")]
    public async Task NotEqual_MultipleOverrides_ExpectToBeNotEqual(string right, string left)
    {
        var c1 = CorrelationId.FromString(right);
        var c2 = CorrelationId.FromString(left);

        await Assert.That(!c1.Equals(c2)).IsTrue();
        await Assert.That(c1 != c2).IsTrue();
        await Assert.That(c2).IsNotEqualTo(c1);
    }
    [Test]
    public async Task Empty_ExpectTrueIfEmpty()
    {
        var correlationId = CorrelationId.Empty;

        await Assert.That(correlationId.IsEmpty).IsTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task FromEmptyString_ExpectTrueIfEmpty(string? value)
    {
        var correlationId = CorrelationId.FromString(value);

        await Assert.That(correlationId.IsEmpty).IsTrue();
    }

    [Test]
    public async Task FromString_ExpectNotToBeEmpty()
    {
        var correlationId = CorrelationId.FromString("123");

        await Assert.That(correlationId.IsEmpty).IsFalse();
    }
}