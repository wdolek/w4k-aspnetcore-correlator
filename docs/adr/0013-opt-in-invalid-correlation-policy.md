# 13. Opt-in policy for handling invalid correlation values (`OnInvalid`)

- Status: Accepted
- Date: 2026-09-09
- Tags: `validation`, `api-design`, `security`, `options`
- Amends: [ADR 0004](0004-opt-in-validation-typed-context.md)

## Context and Problem Statement

ADR 0004 defined a single behavior for invalid correlation values: an `InvalidCorrelationContext`
with an empty correlation ID is created, the value is not propagated, and the request proceeds
normally. Two gaps remained:

1. **Traceability:** on invalid value no new correlation ID is generated, so the offending request
   continues without any correlation - exactly for (potentially malicious) traffic.
2. **Contract:** hardened deployments have no way to reject requests carrying untrusted
   correlation values outright.

The library's non-breaking policy (improvement plan, 2026-09-09) forbids changing the default
behavior.

## Decision Drivers

- Default behavior must not change (non-breaking policy).
- New behavior must be free when not opted in (high-availability goal, ADR 0004).
- One policy knob covering both traceability (regenerate) and hardening (reject).

## Considered Options

- Keep single behavior (ADR 0004 as-is)
- Always regenerate on invalid (breaking change)
- Always reject on invalid (breaking change)
- Opt-in policy on options (`CorrelatorOptions.OnInvalid`)

## Decision Outcome

Chosen option: "Opt-in policy on options (`CorrelatorOptions.OnInvalid`)".

`InvalidCorrelationPolicy` with three values, default preserving existing behavior:

- `KeepEmpty` (default, unchanged): `InvalidCorrelationContext`, empty correlation ID, request
  proceeds (ADR 0004 behavior).
- `GenerateNew`: invalid value is replaced by a newly generated ID, wrapped in a new
  `RegeneratedCorrelationContext` carrying the original header name and validation result.
  Requires a configured correlation ID factory; falls back to `KeepEmpty` otherwise. The
  regenerated value is propagated under the original (incoming) header name for
  `KeepIncomingHeaderName` propagation - the caller sees the replacement value under the header
  they sent.
- `Reject`: the middleware short-circuits the request with HTTP 400 before the rest of the
  pipeline runs.

All policies are relevant only when a validator is registered; without one, no value is ever
invalid and the option has no effect (zero cost).

### Consequences

- **Positive:** traceability gap closed for opted-in deployments; hardened deployments get an
  explicit contract; zero impact otherwise.
- **Positive:** `RegeneratedCorrelationContext` keeps origin header and validation result
  available to custom components (logging, metrics, API responses).
- **Negative:** the context-type hierarchy grows - custom factories/emitters must consider one
  more context type when switching.
- **Negative:** `Reject` couples correlation quality to request success - deliberate, but only
  for consumers who ask for it.

## Pros and Cons of the Options

### Keep single behavior

No new API surface, but the traceability gap and the hardening use case remain unaddressed.

### Always regenerate on invalid

Fixes traceability for everyone, but changes behavior for all consumers with a validator
registered (new IDs where empty ones were before) - violates the non-breaking policy.

### Always reject on invalid

Strongest contract, but turns a logging concern into request failure for every validated
deployment - violates the non-breaking policy.

### Opt-in policy on options

Explicit, documented, free when unused; costs one option and one new context type.
