# 11. Multi-target current .NET support waves with version-locked extension packages

- Status: Accepted
- Date: 2026-09-08
- Tags: `build`, `versioning`, `nuget`

## Context and Problem Statement

ASP.NET Core consumers run on multiple .NET versions at any time (currently .NET 8 LTS, .NET 9,
.NET 10). The library depends on `Microsoft.Extensions.*` packages whose minor versions are tied to
the runtime version. Additionally, per-TFM performance optimizations exist (sanitizer
implementations, ADR 0007), and .NET framework-sandboxing features (package pruning) interact with
older TFMs.

## Decision Drivers

- One NuGet package serving all supported .NET versions.
- Consumers on each TFM get the matching `Microsoft.Extensions.*` dependency (no unnecessary
  upgrades forced by the library).
- Room for per-TFM optimizations.

## Considered Options

- Single lowest-common-denominator TFM (`net8.0`) with floating dependency versions
- Multi-target each supported .NET wave with exact `[Major.Minor,)` pins per TFM
- Target only latest .NET

## Decision Outcome

Chosen option: "Multi-target each supported .NET wave with exact `[Major.Minor,)` pins per TFM".

The package targets `net8.0;net9.0;net10.0`, referencing `Microsoft.Extensions.Http`, `.Options`
and `.Options.DataAnnotations` pinned per TFM (`[8.0.0,)`, `[9.0.0,)`, `[10.0.0,)` respectively).
Framework-dependent code paths (sanitizer) use `#if NET8_0` / `#if NET9_0_OR_GREATER`. Package
pruning is disabled while `net8.0` is supported. Unit tests multi-target all three TFMs;
integration tests target `net10.0` only; CI builds/tests on `ubuntu-latest`.

### Consequences

- **Positive:** each consumer TFM resolves the matching (or newer) framework packages - no forced
  dependency uplift, no downlevel mismatches.
- **Positive:** per-TFM code paths are possible without runtime feature switches.
- **Negative:** build/test matrix grows with every .NET release; conditional ItemGroups in the
  csproj are error-prone boilerplate.
- **Negative:** integration tests cover only `net10.0` - behavior differences across TFMs
  (especially in `AsyncLocal`/Kestrel header handling) are only covered indirectly by multi-TFM
  unit tests.
- **Negative:** CI runs Linux only; Windows/macOS-specific behavior is untested.
