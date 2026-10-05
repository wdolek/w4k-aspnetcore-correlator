using System.Linq;
using Microsoft.Extensions.Options;
using W4k.AspNetCore.Correlator.Http;

namespace W4k.AspNetCore.Correlator.Options;

internal sealed class CorrelatorOptionsValidator : IValidateOptions<CorrelatorOptions>
{
    public ValidateOptionsResult Validate(string? name, CorrelatorOptions options)
    {
        if (options.ReadFrom.Count == 0)
        {
            return ValidateOptionsResult.Fail(
                $"Configure at least one correlation HTTP header, see property: {nameof(CorrelatorOptions.ReadFrom)}");
        }

        var invalidHeaderNames = options.ReadFrom
            .Where(headerName => !HeaderNameValidator.IsValidHeaderName(headerName))
            .ToArray();

        if (invalidHeaderNames.Length > 0)
        {
            return ValidateOptionsResult.Fail(
                $"Invalid correlation HTTP header name(s) configured: {string.Join(", ", invalidHeaderNames)}, see property: {nameof(CorrelatorOptions.ReadFrom)}");
        }

        return ValidateOptionsResult.Success;
    }
}
