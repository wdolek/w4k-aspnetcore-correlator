using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelatedLoggerStateTests
{
    [Test]
    public async Task Ctor_ExpectStringValueAndStructure()
    {
        // arrange
        var correlationKey = "CorrelationKey";
        var correlationId = CorrelationId.FromString("correlation_id");

        // act
        var state = new CorrelatedLoggerState("CorrelationKey", correlationId);

        // assert
        await Assert.That(state.ToString()).IsEqualTo("CorrelationKey:correlation_id");

        await Assert.That(state).IsAssignableTo<IEnumerable<KeyValuePair<string, object>>>();

        var stateArray = state.ToArray();
        await Assert.That(stateArray).HasSingleItem();
        await Assert.That(stateArray[0].Key).IsEqualTo(correlationKey);
        await Assert.That(stateArray[0].Value).IsEqualTo(correlationId);
    }
}