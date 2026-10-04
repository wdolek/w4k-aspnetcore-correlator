namespace W4k.AspNetCore.Correlator.Options;

/// <summary>
/// Policy applied when received correlation value is found invalid by registered correlation validator.
/// </summary>
/// <remarks>
/// Policy is relevant only when correlation validator is registered
/// (<see cref="W4k.AspNetCore.Correlator.Validation.ICorrelationValidator"/>). When no validator
/// is registered, received values are accepted as-is and this policy has no effect.
/// </remarks>
public enum InvalidCorrelationPolicy
{
    /// <summary>
    /// Invalid value results in <see cref="W4k.AspNetCore.Correlator.Context.Types.InvalidCorrelationContext"/>
    /// with empty correlation ID - request is processed without correlation ID. Default policy.
    /// </summary>
    KeepEmpty = 0,

    /// <summary>
    /// Invalid value is replaced by a newly generated correlation ID, resulting in
    /// <see cref="W4k.AspNetCore.Correlator.Context.Types.RegeneratedCorrelationContext"/>.
    /// When correlation ID factory is not configured, falls back to <see cref="KeepEmpty"/>.
    /// </summary>
    GenerateNew,

    /// <summary>
    /// Request is rejected with HTTP 400 before processing.
    /// </summary>
    Reject,
}
