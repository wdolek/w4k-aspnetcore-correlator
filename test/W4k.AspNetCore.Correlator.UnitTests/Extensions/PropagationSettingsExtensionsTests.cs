using System.Collections.Generic;
using W4k.AspNetCore.Correlator.Extensions;
using W4k.AspNetCore.Correlator.Options;
using Xunit;

namespace W4k.AspNetCore.Correlator.UnitTests.Extensions
{
    public class PropagationSettingsExtensionsTests
    {
        [Theory]
        [MemberData(nameof(Generate))]
        public void GetCorrelationHeaderName_ExpectValue(
            PropagationSettings propagation,
            string incomingHeader,
            string expected)
        {
            var headerName = propagation.GetCorrelationHeaderName(incomingHeader);
            Assert.Equal(expected, headerName);
        }

        public static IEnumerable<object[]> Generate()
        {
            yield return new object[] { PropagationSettings.NoPropagation, "X-Dummy", null };

            yield return new object[]
            {
                PropagationSettings.KeepIncomingHeaderName,
                "X-Fabulous-Correlation-ID",
                "X-Fabulous-Correlation-ID"
            };

            yield return new object[]
            {
                PropagationSettings.PropagateAs("X-Correlation-ID"),
                "X-Fabulous-Correlation-ID",
                "X-Correlation-ID"
            };
        }
    }
}
