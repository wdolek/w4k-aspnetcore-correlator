using System.Collections.Generic;
using W4k.AspNetCore.Correlator.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Options
{
    public class PropagationSettingsTests
    {
        [Theory]
        [MemberData(nameof(GenerateDefaultPropagationSettings))]
        public void Ctor_InstantiatedUsingDefault_ExpectNoPropagation(PropagationSettings propagation)
        {
            Assert.Equal(HttpHeaders.CorrelationId, propagation.HeaderName);
            Assert.Equal(HeaderPropagation.NoPropagation, propagation.Settings);
        }

        public static IEnumerable<object[]> GenerateDefaultPropagationSettings()
        {
            yield return new object[] { new PropagationSettings() };
            yield return new object[] { default(PropagationSettings) };
        }
    }
}
