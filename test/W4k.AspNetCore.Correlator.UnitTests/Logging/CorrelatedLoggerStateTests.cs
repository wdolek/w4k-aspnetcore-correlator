using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace W4k.AspNetCore.Correlator.Logging;

public class CorrelatedLoggerStateTests
{
    [Fact]
    public void Ctor_ExpectStringValueAndStructure()
    {
        // arrange
        var correlationKey = "CorrelationKey";
        var correlationId = CorrelationId.FromString("correlation_id");

        // act
        var state = new CorrelatedLoggerState("CorrelationKey", correlationId);

        // assert
        Assert.Equal("CorrelationKey:correlation_id", state.ToString());

        Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object>>>(state);

        var stateArray = state.ToArray();
        Assert.Single(stateArray);
        Assert.Equal(correlationKey, stateArray[0].Key);
        Assert.Equal(correlationId, stateArray[0].Value);
    }
}