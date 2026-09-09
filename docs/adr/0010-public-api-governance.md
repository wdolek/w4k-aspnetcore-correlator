# 10. Public API governance via analyzers and root-namespace extensions

- Status: Accepted
- Date: 2026-09-08
- Tags: `api-design`, `versioning`, `tooling`

## Context and Problem Statement

Correlator is a NuGet library whose extension methods are its primary surface. Discovering
extension methods depends on their namespace, and evolving a public API without accidental
breakage requires tooling. Historically the library used two namespaces
(`W4k.AspNetCore.Correlator` and `W4k.AspNetCore.Correlator.Extensions.DependencyInjection`), which
fragmented discoverability: types were importable but their extension methods were not in the
"natural" namespace.

## Decision Drivers

- Extension methods must be discoverable when the consumer imports the types' namespace.
- Public API changes must be caught at compile time (including nullability annotations).
- Breaking changes must follow a deprecation path, not silent removal.

## Considered Options

- Keep dual namespaces indefinitely
- Move extensions to root namespace, delete old namespace outright
- Move extensions to root namespace, keep old ones as `[Obsolete]` forwarders
- No governance tooling

## Decision Outcome

Chosen option: "Move extensions to root namespace, keep old ones as `[Obsolete]` forwarders" +
"Public API tracking via `Microsoft.CodeAnalysis.PublicApiAnalyzers`".

All extension methods live in `namespace W4k.AspNetCore.Correlator` (same as the types they extend
or configure); the previous `W4k.AspNetCore.Correlator.Extensions.DependencyInjection` variants
remain as `[Obsolete]` pass-throughs. `PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt` per target
framework track the exact public surface, so any (even nullability) change is a build error until
acknowledged.

### Consequences

- **Positive:** `using W4k.AspNetCore.Correlator;` brings types and their extensions together -
  best discoverability for the primary scenario.
- **Positive:** shipped/unshipped API files make accidental additions/changes impossible to miss,
  and serve as living API reference.
- **Negative:** duplicated extension implementations (forwarders) double the API surface until the
  obsolete set is removed in a future major.
- **Negative:** XML-doc maintenance burden on the forwarders; `PublicAPI.Unshipped.txt` currently
  mixes the (already released) forwarders with genuinely new API - hygiene work needed.
