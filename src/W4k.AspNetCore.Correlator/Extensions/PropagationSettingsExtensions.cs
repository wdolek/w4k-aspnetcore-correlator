using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions
{
    public static class PropagationSettingsExtensions
    {
        /// <summary>
        /// Decides HTTP header to emit correlation ID.
        /// </summary>
        /// <remarks>
        /// On missing incoming header/invalid setting, header name defaults to <see cref="HttpHeaders.CorrelationId"/>.
        /// </remarks>
        /// <param name="settings">Header propagation settings.</param>
        /// <param name="incomingHeaderName">Request HTTP header containing correlation ID.</param>
        /// <returns>
        /// Returns name of header used for emitting correlation ID (either <paramref name="incomingHeaderName"/>
        /// or predefined by <see cref="PropagationSettings"/>) or <c>null</c> if header should not be propagated.
        /// </returns>
        public static string GetCorrelationHeaderName(this PropagationSettings settings, string incomingHeaderName)
        {
            if (settings == PropagationSettings.NoPropagation)
            {
                return null;
            }

            var headerName = settings == PropagationSettings.KeepIncomingHeaderName
                ? incomingHeaderName
                : settings.HeaderName;

            return headerName ?? HttpHeaders.CorrelationId;
        }
    }
}
