using System;
using System.Diagnostics.CodeAnalysis;

namespace W4k.AspNetCore.Correlator;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOpException(string message) =>
        throw new InvalidOperationException(message);
}