# 3. Single middleware handling read, validate, generate and integration points

- Status: Accepted
- Date: 2026-09-08
- Tags: `middleware`, `pipeline`, `aspnetcore`

## Context and Problem Statement

Correlation ID handling consists of several per-request concerns: reading incoming headers,
validating, generating a fallback value, exposing the result to the request pipeline, optionally
replacing `HttpContext.TraceIdentifier`, and optionally creating a logger scope. These could be
separate middlewares or one middleware.

## Decision Drivers

- Correlation ID should be available as early as possible in the pipeline (register first).
- Minimal per-request overhead.
- Predictable, single registration point for users.

## Considered Options

- One middleware for everything
- Multiple middlewares (reader, generator, logger scope, emitter)
- No middleware: handle correlation in `IHttpApplication` / endpoint filters

## Decision Outcome

Chosen option: "One middleware for everything" (`CorrelatorMiddleware`).

The middleware:
1. creates the correlation scope via `ICorrelationScopeFactory` (read -> validate -> generate),
2. registers response emission via `Response.OnStarting` when `Emit` is enabled (ADR 0008),
3. optionally replaces `TraceIdentifier` with the correlation ID,
4. optionally opens a logger scope (`CorrelatedLoggerState`) around the rest of the pipeline,
5. disposes the scope on completion, restoring the ambient context (ADR 0002).

### Consequences

- **Positive:** single `UseCorrelator()` call; correlation established at one point, as early in
  the pipeline as the user places it.
- **Positive:** scope creation and disposal are balanced in one place - no leaked `AsyncLocal` state.
- **Negative:** feature flags (`Emit`, `ReplaceTraceIdentifier`, `LoggingScope`) make the middleware
  a small "god object"; each option adds branches to the hot path.
- **Negative:** users forgetting to register the middleware early lose correlation for logging of
  earlier middlewares - a misuse mode that cannot be prevented by the library.

## Pros and Cons of the Options

### One middleware for everything

Fewest moving parts and guaranteed balanced scope lifetime; options-checked per request
(skipping disabled features is cheap).

### Multiple middlewares

Compositional, but ordering mistakes (emitter before reader, logger scope not wrapping handlers)
become user errors, and each middleware adds pipeline overhead.

### No middleware

Moving into hosting/endpoint layers would break minimal-API style apps and lose ordering control.
