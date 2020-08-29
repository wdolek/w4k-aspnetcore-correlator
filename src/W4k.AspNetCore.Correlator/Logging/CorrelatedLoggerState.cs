namespace W4k.AspNetCore.Correlator.Logging
{
    internal sealed class CorrelatedLoggerState
    {
        private readonly string _key;
        private readonly CorrelationId _correlationId;

        public CorrelatedLoggerState(string key, CorrelationId correlationId)
        {
            _key = key;
            _correlationId = correlationId;
        }

        public override string ToString() => $"{_key}:{_correlationId.Value}";
    }
}
