# Architecture Decision Records (ADR)

Decisions for [W4k.AspNetCore.Correlator](https://github.com/wdolek/w4k-aspnetcore-correlator) are documented
using the [MADR](https://adr.github.io/madr/) format (Markdown Any/Decision Records).

> **Note:** ADRs 0001-0012 are retrospective - they document decisions already implemented in the library
> (as of v3.4.0) in order to make them explicit and enable future changes to be evaluated against them.
> New decisions (0013 onwards) are added **before** implementation, following the same format.

| # | Title | Status |
|---|-------|--------|
| [0001](0001-simple-non-standard-correlation-headers.md) | Use simple non-standard correlation headers | Accepted |
| [0002](0002-asynclocal-ambient-correlation-context.md) | Flow correlation context via `AsyncLocal` | Accepted |
| [0003](0003-single-middleware-pipeline.md) | Single middleware handling read, validate, generate and integration points | Accepted |
| [0004](0004-opt-in-validation-typed-context.md) | Opt-in validation producing typed context, never rejecting requests | Accepted |
| [0005](0005-propagation-settings-model.md) | Propagation modeled as mode + header name (`PropagationSettings`) | Accepted |
| [0006](0006-pluggable-correlation-id-factory.md) | Correlation ID generation via pluggable factory delegate | Accepted |
| [0007](0007-log-value-sanitization.md) | Sanitize correlation values before logging | Accepted |
| [0008](0008-response-emission-on-starting.md) | Emit correlation ID to response in `OnStarting`, never overwriting | Accepted |
| [0009](0009-single-registration-di-components.md) | Single-registration DI components with fail-fast guards | Accepted |
| [0010](0010-public-api-governance.md) | Public API governance via analyzers and root-namespace extensions | Accepted |
| [0011](0011-multi-targeting-strategy.md) | Multi-target current .NET support waves with version-locked extension packages | Accepted (amended 2026-11-11: net10.0/net11.0, framework-reference-only deps) |
| [0012](0012-minimal-dependency-footprint.md) | Minimal dependency footprint - no hosting, lazy options validation | Accepted |
| [0013](0013-opt-in-invalid-correlation-policy.md) | Opt-in policy for invalid correlation values (`OnInvalid`) | Accepted |

Proposals for changing or extending these decisions are tracked in [../plans](../plans).
