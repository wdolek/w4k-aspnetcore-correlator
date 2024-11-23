using System.Collections;
using System.Collections.Generic;

namespace W4k.AspNetCore.Correlator.Logging;

internal sealed class CorrelatedLoggerState : IEnumerable<KeyValuePair<string, object>>
{
    private readonly List<KeyValuePair<string, object>> _value;

    public CorrelatedLoggerState(string key, CorrelationId correlationId)
    {
        _value = [new KeyValuePair<string, object>(key, correlationId)];
    }

    public override string ToString() => $"{_value[0].Key}:{_value[0].Value}";

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _value.GetEnumerator();
}