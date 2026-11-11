# 12. Minimal dependency footprint - no hosting, no options-startup validation

- Status: Accepted
- Date: 2026-09-08
- Tags: `dependencies`, `nuget`, `performance`, `scope`

## Context and Problem Statement

Correlator is a small cross-cutting library for high-availability APIs. Every package dependency it
takes is transitively forced onto every consumer, and every framework subsystem it touches at
startup adds friction (or load) to application boot. Features that look "free" often require new
dependencies - e.g. eager options validation (`ValidateOnStart`) requires the
hosting/startup-validator machinery (`Microsoft.Extensions.Hosting`), which Correlator otherwise
does not need at all.

## Decision Drivers

- Keep the transitive dependency graph as small as the functionality requires.
- No per-request overhead (see ADR 0004); startup machinery is acceptable only if dependency-free.
- Library must work in any ASP.NET Core host (minimal API, classic `Startup`, custom hosts).

## Considered Options

- Minimal: depend only on `Microsoft.Extensions.Http` + `Microsoft.Extensions.Options`
- Adopt `Microsoft.Extensions.Options.DataAnnotations` for declarative option validation
- Adopt `Microsoft.Extensions.Hosting` for `ValidateOnStart`-style eager validation
- Full options-binding subsystem for `IConfiguration` support

## Decision Outcome

Chosen option: "Minimal: depend only on `Microsoft.Extensions.Http` + `Microsoft.Extensions.Options`".

The library uses only the DI/options/HTTP/logging abstractions that ASP.NET Core already provides.
As a consequence:

- Options validation is registered via the plain `Validate(...)` callback (already part of
  `Microsoft.Extensions.Options`) and therefore runs **lazily**, when options are first resolved -
  not at application start. Eager validation would require hosting dependencies
  (`ValidateOnStart`), which is rejected. Consumers who want eager validation can call
  `ValidateOnStart()` themselves in their own host - it is a consumer decision, not a library one.
- The `Microsoft.Extensions.Options.DataAnnotations` package was unused and has been removed in
  4.0.0 (improvement plan A6) - options rules are expressed with plain
  `Validate(...)` callbacks.
- `IConfiguration` binding of options is not provided by the library (delegate-based `Factory`,
  readonly-struct settings - see improvement plan A5 for optional patterns); adding a binding
  subsystem is not worth the footprint.
- Log sanitization was implemented in-library rather than relying on external logger packages
  (ADR 0007) - same principle.

### Consequences

- **Positive:** consumers' dependency graphs stay clean; no version conflicts with hosting
  packages; works in minimal and custom hosting scenarios.
- **Positive:** no hidden startup registrations (hosted services, startup validators) beyond what
  the user explicitly adds.
- **Negative:** misconfiguration (e.g. empty `ReadFrom`) surfaces on first options use rather than
  at startup - accepted, considered obvious to .NET users.
- **Negative:** configuration binding is left as an exercise/pattern instead of a built-in
  convenience.
