# 4. Opt-in validation producing a typed context, never rejecting requests

- Status: Accepted
- Date: 2026-09-08
- Tags: `validation`, `api-design`, `security`, `performance`

## Context and Problem Statement

Incoming correlation headers are attacker-controlled input. Unbounded or malicious values can be
echoed to responses, forwarded to downstream services, written into logs, or - with
`ReplaceTraceIdentifier` - injected into platform trace logs. The library needs a policy for
handling values it cannot trust - while serving **high-availability APIs**, where per-request
overhead must be as close to zero as possible.

Many consumers run on trusted networks (intranet, service meshes, internal gateways) where
correlation headers are set by friendly infrastructure. For those deployments, any unconditional
validation cost is pure waste on the hot path.

## Decision Drivers

- Must not break existing consumers for whom correlation is best-effort.
- Must allow security-conscious consumers (internet-facing APIs) to stop untrusted values from
  propagating.
- Zero per-request cost when validation is not needed - the library targets high-availability APIs.
- Failed validation must never turn a logging concern into a user-facing failure.

## Considered Options

- No validation, consumers sanitize themselves
- Opt-in `ICorrelationValidator`, invalid value results in `InvalidCorrelationContext`
- Opt-in validation, invalid value rejects the request (400)
- Always-on length validation (fail-safe default)

## Decision Outcome

Chosen option: "Opt-in `ICorrelationValidator`, invalid value results in `InvalidCorrelationContext`".

No validator is registered by default and **no validation code runs by default** - deployments on
trusted networks pay nothing. When a validator is registered, `CorrelationContextFactory` validates
the first raw header value; an invalid value produces an `InvalidCorrelationContext` carrying the
header name and `ValidationResult` (with reason). The correlation ID of an invalid context is
`CorrelationId.Empty`, so the value is **not** propagated or forwarded; the request itself proceeds
normally. The library ships a lightweight `CorrelationValueLengthValidator` (max length, empty
considered invalid).

The remaining log-injection surface (correlator's own log output) is closed unconditionally and
cheaply by sanitization at the logging boundary (ADR 0007) - guarded by `IsEnabled` so it also
costs nothing when info logging is off.

### Consequences

- **Positive:** failed validation never degrades the request; correlation is dropped, not the
  business call.
- **Positive:** trusted-network deployments get a validation-free hot path; hardened deployments
  opt in with a single line (`.WithValidator(...)`).
- **Positive:** typed `InvalidCorrelationContext` lets custom factories/emitters/loggers react to
  invalid values explicitly.
- **Positive:** `ICorrelationValidator` is a single-method extension point - consumers can implement
  charset/format policies.
- **Negative:** by default (no validator) unbounded input is accepted, echoed, forwarded and used as
  `TraceIdentifier`. This is an accepted trade-off in exchange for the zero-overhead default;
  internet-facing deployments must opt in (see improvement plan: security guidance + opt-in
  validator helpers, e.g. a shipped pattern/charset validator reusing the safe-char allowlist).
- **Negative:** on invalid value no new correlation ID is generated - the request continues
  *without* correlation, which harms traceability for the offending request (improvement plan:
  opt-in invalid-value policy, active only when a validator is registered).

## Pros and Cons of the Options

### No validation

Simplest and fastest, but knowingly propagates untrusted input even for internet-facing consumers;
hardened deployments would have to hand-roll sanitization.

### Opt-in validation with `InvalidCorrelationContext`

Balances safety (value dropped when opted in) with availability and zero default cost; validation
policy is the consumer's call, matching the deployment's trust model.

### Opt-in validation rejecting requests (400)

Clear signal to misbehaving callers, but changes request semantics based on a logging concern;
correlation is never worth failing a business request for. (Could be offered later as an opt-in
policy, not as the only behavior.)

### Always-on length validation

Safer-by-default, but charges every request of every consumer - including trusted intranet
services - for a threat many of them do not face; conflicts with the high-availability goal.
