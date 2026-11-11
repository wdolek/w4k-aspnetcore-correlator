# 11. Multi-target current .NET support waves with version-locked extension packages

- Status: Accepted (amended 2026-11-11)
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

## Amendment (2026-11-11): framework-reference-only dependencies, net10.0/net11.0

Executed as R1 of the improvement plan, one day after net8.0/net9.0 reached end of support
(2026-11-10). The multi-targeting strategy itself stands - the package still targets each
supported .NET wave. What changed:

- **Target frameworks:** `net10.0;net11.0`. Older waves are dropped as they go out of support;
  the newest two remain (net10.0 is LTS until 2028, net11.0 is the current STS wave).
- **Per-TFM version-locked `Microsoft.Extensions.*` `PackageReference` ItemGroups removed.** The
  project uses the Web SDK, so `Microsoft.Extensions.Http`/`Options` (and the other abstractions
  the library uses, ADR 0012) are provided by the `Microsoft.AspNetCore.App` framework reference,
  which also flows to consumers. The nuspec dependency groups are now empty (framework reference
  only). The previous explicit pins were redundant for real consumers: every consumer is an
  ASP.NET Core app whose shared framework already contains matching (or newer) versions, and the
  framework reference was already part of the package in the pinned era. The .NET 11 SDK makes
  this explicit: package pruning (NU1510) prunes such references by default - the temporary
  `RestoreEnablePackagePruning=false` workaround was dropped rather than reinstated.
- **`Microsoft.Extensions.Options.DataAnnotations` removed** (unused, improvement plan A6).
- **Sanitizer:** the net8.0-specific implementation (ADR 0007) was deleted; the single
  `SearchValues`-based implementation (formerly `NET9_0_OR_GREATER`) serves all TFMs.
- **Tests/benchmarks:** unit tests multi-target both TFMs; integration tests target `net11.0`
  only; benchmarks compare net10.0 (baseline) vs net11.0 jobs.

Consequences of the amendment:

- **Positive:** no more error-prone conditional ItemGroups (a "Negative" of the original
  decision); nuspec can never disagree with a consumer's shared framework version.
- **Positive:** smaller dependency graph (ADR 0012) - zero `Microsoft.Extensions.*` version
  requirements forced onto consumers.
- **Negative:** a hypothetical non-ASP.NET Core consumer now resolves `Microsoft.Extensions.*`
  purely via the shared framework instead of NuGet pins - irrelevant in practice (the library is
  ASP.NET Core middleware and always required the framework reference anyway).
