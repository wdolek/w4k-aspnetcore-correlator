# 8. Emit correlation ID to response in `OnStarting`, never overwriting

- Status: Accepted
- Date: 2026-09-08
- Tags: `http`, `response`, `aspnetcore`

## Context and Problem Statement

When `CorrelatorOptions.Emit` is enabled, the correlation ID should be returned to the caller in a
response header. Response headers can be set until the response starts; middleware runs before the
endpoint executes, so the emit can happen either immediately (before the rest of the pipeline runs)
or deferred to `Response.OnStarting`.

## Decision Drivers

- Application code setting its own correlation header must win over the middleware.
- Emitting must not throw for responses started early by the endpoint (headers already locked).
- No per-response-header overwrite: "first value wins".

## Considered Options

- Set response header immediately in the middleware, before invoking the pipeline
- Defer to `Response.OnStarting` callback, using add-if-not-set
- Emit in `OnCompleted`

## Decision Outcome

Chosen option: "Defer to `Response.OnStarting` callback, using add-if-not-set".

The middleware registers `httpContext.Response.OnStarting(...)` that calls
`ICorrelationEmitter.Emit`, which writes the header via `AddHeaderIfNotSet` (a `TryAdd`). Because
the callback runs just before headers are flushed, any header set earlier by the application takes
precedence; the correlator only fills the gap. Emission is skipped entirely for empty/invalid
correlation contexts.

### Consequences

- **Positive:** "app wins, correlator fills in" semantics - sensible for custom emitters and apps
  that set correlation headers deliberately.
- **Positive:** works regardless of where in the pipeline `UseCorrelator()` is placed.
- **Negative:** one closure allocation per request on the hot path (capturing `httpContext` and
  the context) - see improvement plan.
- **Negative:** if the endpoint starts the response *and* the OnStarting callback throws (e.g.
  invalid header value rejected by the server), the failure surfaces late, as a 500.
