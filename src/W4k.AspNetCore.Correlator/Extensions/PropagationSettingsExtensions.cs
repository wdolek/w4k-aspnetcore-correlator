using System;
using W4k.AspNetCore.Correlator.Options;

namespace W4k.AspNetCore.Correlator.Extensions
{
    /// <summary>
    /// Extensions of <see cref="PropagationSettings"/>.
    /// </summary>
    public static class PropagationSettingsExtensions
    {
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <c>null</c>.</exception>
        private static PropagationSettings On(
            this PropagationSettings propagation,
            HeaderPropagation targetPropagation,
            Action<PropagationSettings> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (propagation.Settings == targetPropagation)
            {
                action(propagation);
            }

            return propagation;
        }
    }
}
