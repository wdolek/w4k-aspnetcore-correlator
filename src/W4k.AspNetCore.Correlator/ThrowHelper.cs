using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1574
namespace W4k.AspNetCore.Correlator;

/// <summary>
/// Helper class to support older .NET targets, where <see cref="ArgumentException.ThrowIfNullOrWhiteSpace"/> are not available.
/// </summary>
internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOp(string message) => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowArgNull(string? paramName) => throw new ArgumentNullException(paramName);

    public static void ThrowIfNull(object? argument, string? paramName = null)
    {
        if (argument is null)
        {
            ThrowArgNull(paramName);
        }
    }
}