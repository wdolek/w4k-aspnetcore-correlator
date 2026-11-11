# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Releases up to and including 3.3.0 are documented on the
[GitHub releases page](https://github.com/wdolek/w4k-aspnetcore-correlator/releases).

## [4.0.0]

### Added

- .NET 11 (`net11.0`) target

### Changed

- **Breaking:** target frameworks are now `net10.0` and `net11.0` only (previously `net8.0`, `net9.0` and `net10.0`)
- Package no longer declares `Microsoft.Extensions.*` dependencies: the required abstractions are provided by the `Microsoft.AspNetCore.App` shared framework reference (which consumers already required transitively), see ADR 0011 amendment
- Single log-value sanitizer implementation for all target frameworks (net8.0-specific implementation removed)

### Removed

- **Breaking:** `net8.0` and `net9.0` support (both out of support since 2026-11-10)
- `Microsoft.Extensions.Options.DataAnnotations` dependency (unused, see improvement plan A6)

### Fixed

- Debug builds of a Web SDK app project-referencing this library failed on .NET 11 SDK with "scoped css bundles do not have matching endpoints" (SDK regression, [dotnet/sdk#56656](https://github.com/dotnet/sdk/issues/56656)) - worked around with `EnableHotReloadInRuntimeConfigDevFile=false`, to be removed once the SDK fix ships

## [3.7.0]

### Changed

- Header names within `CorrelatorOptions.ReadFrom` are validated when options are first resolved, invalid header name causes options validation failure (previously silently ignored)
- Header names passed to `PropagationSettings.PropagateAs`/`KeepIncomingHeaderName` are validated when the settings are created, `ArgumentException` is thrown on invalid header name

## [3.6.0]

### Added

- `CorrelatorOptions.InvalidValuePolicy`: policy applied when received correlation value is found invalid
  by registered correlation validator (`KeepEmpty` by default, `GenerateNew` or `Reject`)

## [3.5.0] - 2026-09-13

### Added

- `CorrelationValuePatternValidator`: shipped default validator accepting non-empty values up to 80 characters consisting of characters safe for logging only
- `WithDefaultValidator()`: shorthand for registering the shipped default validator
- Security guidance: correlation header values treated as attacker-controlled input, see [README](README.md#security)

## [3.4.0] - 2025-11-18

### Added

- Explicit .NET 10 support (#69)
