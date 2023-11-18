using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable 1574
namespace W4k.AspNetCore.Correlator;

/// <summary>
/// Helper class to support older .NET targets, where <see cref="ArgumentException.ThrowIfNullOrEmpty"/> is not available.
/// </summary>
internal static class ThrowHelper
{
    public static void ThrowIfNullOrEmpty(string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            ThrowNullOrEmptyException(argument, paramName);
        }
    }

    [DoesNotReturn]
    public static void ThrowInvalidOpException(string message) =>
        throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowNullOrEmptyException(string? argument, string? paramName)
    {
        ArgumentNullException.ThrowIfNull(argument);
        throw new ArgumentException("The value cannot be an empty string.", paramName);
    }
}