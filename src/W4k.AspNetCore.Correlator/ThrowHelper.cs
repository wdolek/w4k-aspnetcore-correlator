using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1574
namespace W4k.AspNetCore.Correlator
{
    /// <summary>
    /// Helper class to support <c>netstandard</c> targets, where <see cref="ArgumentNullException.ThrowIfNull(object?, string?)"/> is not available.
    /// </summary>
    internal static class ThrowHelper
    {
#if NET6_0_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowInvalidOp(string message) => throw new InvalidOperationException(message);

#if NET6_0_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowArgNull(string? paramName) => throw new ArgumentNullException(paramName);

        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgNull(paramName);
            }
        }
    }
}
