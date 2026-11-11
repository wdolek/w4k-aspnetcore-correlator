# 7. Sanitize correlation values before logging

- Status: Accepted
- Date: 2026-09-08
- Tags: `security`, `logging`, `performance`

## Context and Problem Statement

The correlation header value is attacker-controlled. Correlator logs it (`CorrelationIdReceived`,
event 203). Logging unsanitized header values enables log forging/injection (newlines, ANSI escape
sequences), bloats logs (megabyte-long values) and can leak sensitive content into log stores.

## Decision Drivers

- Log injection must be impossible by construction.
- Hot path cost must be negligible; sanitized logging must not penalize disabled log levels.
- Sanitization must not alter the correlation ID actually used by the request.

## Considered Options

- Log raw values, let the logging framework deal with it
- Truncate only (length cap)
- Replace non-allowlisted characters (`*`) with an allowlist of "safe" chars, capped at 80 chars
- Never log the value at all

## Decision Outcome

Chosen option: "Replace non-allowlisted characters with an allowlist of safe chars, capped at 80
chars".

`CorrelationIdValueSanitizer.Sanitize` allowlists `#+-./0-9:=A-Z_a-z|~`, replaces any other
character with `*`, and truncates the output at 80 characters. It is applied **only** at the logging
boundary (`LoggerExtensions.CorrelationIdReceived`), after an `IsEnabled(LogLevel.Information)`
check; the in-flight correlation value itself is never modified. Two per-TFM implementations
(`SearchValues<char>`-based, net8 vs. net9+ `string.Create` with ref-struct state) exist for
performance, with the shared allowlist string as single source of truth.

### Consequences

- **Positive:** log injection through correlation values is structurally prevented in correlator's
  own log output; bounded log line cost regardless of input size.
- **Positive:** fast paths: cheap `IsEnabled` guard; single-allocation sanitize only when info
  logging is on and the value contains unsafe chars or exceeds the cap.
- **Negative:** sanitization covers only correlator's own loggers - the value is still propagated
  unsanitized into user logs via the logging scope, `TraceIdentifier` replacement and the emitted
  response header (see improvement plan: sanitize-at-boundary vs. validate-at-ingest).
- **Negative:** two near-duplicate sanitizer implementations must be kept behaviorally in sync
  (maintainability cost, mitigated by unit tests).
