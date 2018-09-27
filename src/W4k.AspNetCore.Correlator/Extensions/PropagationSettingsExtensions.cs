using System;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions
{
    public static class PropagationSettingsExtensions
    {
        /// <summary>
        /// Decides HTTP header to emit correlation ID.
        /// </summary>
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

            return headerName;
        }

        /// <summary>
        /// Invokes <paramref name="action"/> on <see cref="HeaderPropagation.KeepIncomingHeaderName"/>.
        /// </summary>
        /// <param name="propagation">Propagation settings.</param>
        /// <param name="action">Action to be invoked.</param>
        /// <returns>
        /// Propagation settings.
        /// </returns>
        public static PropagationSettings OnIncomingHeader(
            this PropagationSettings propagation,
            Action<PropagationSettings> action) => propagation.On(HeaderPropagation.KeepIncomingHeaderName, action);

        /// <summary>
        /// Invokes <paramref name="action"/> on <see cref="HeaderPropagation.UsePredefinedHeaderName"/>.
        /// </summary>
        /// <param name="propagation">Propagation settings.</param>
        /// <param name="action">Action to be invoked.</param>
        /// <returns>
        /// Propagation settings.
        /// </returns>
        public static PropagationSettings OnPredefinedHeader(
            this PropagationSettings propagation,
            Action<PropagationSettings> action) => propagation.On(HeaderPropagation.UsePredefinedHeaderName, action);

        /// <summary>
        /// Invokes <paramref name="action"/> on <paramref name="targetPropagation"/>.
        /// </summary>
        /// <param name="propagation">Propagation settings.</param>
        /// <param name="targetPropagation">Target propagation.</param>
        /// <param name="action">Action to be invoked.</param>
        /// <returns>
        /// Propagation settings.
        /// </returns>
        private static PropagationSettings On(
            this PropagationSettings propagation,
            HeaderPropagation targetPropagation,
            Action<PropagationSettings> action)
        {
            if (propagation.Settings == targetPropagation)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                action(propagation);
            }

            return propagation;
        }
    }
}
