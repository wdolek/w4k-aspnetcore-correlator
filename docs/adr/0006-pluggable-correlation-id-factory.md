# 6. Correlation ID generation via pluggable factory delegate

- Status: Accepted
- Date: 2026-09-08
- Tags: `api-design`, `options`, `generation`

## Context and Problem Statement

When no correlation header is received, the library may generate a new correlation ID. Consumers
have varying requirements for ID shape (GUID, ULID, snowflake, tenant-prefixed, ...), and some
deployments want to disable generation entirely (e.g. trust only upstream-originated correlation,
falling back to ASP.NET's `TraceIdentifier`).

## Decision Drivers

- Allow arbitrary ID formats without the library guessing.
- Allow disabling generation.
- Keep the happy path allocation-light.

## Considered Options

- Fixed GUID generation inside the library
- Pluggable `ICorrelationIdGenerator` service
- `Func<HttpContext, CorrelationId>` delegate on options (`CorrelatorOptions.Factory`)

## Decision Outcome

Chosen option: "`Func<HttpContext, CorrelationId>` delegate on options".

`CorrelatorOptions.Factory` defaults to `Guid.NewGuid().ToString("D")` wrapped in `CorrelationId`;
setting it to `null` disables generation (requests without incoming correlation get
`EmptyCorrelationContext`). The delegate receives `HttpContext`, so consumers can derive IDs from
request properties if desired.

### Consequences

- **Positive:** zero new public types; configuration-local; trivially testable.
- **Positive:** receiving `HttpContext` allows derived/synthetic correlation IDs.
- **Negative:** a delegate is not DI-resolvable - consumers needing injected services in the
  factory must close over them before registration (documented pattern).
- **Negative:** delegate failures (throwing factory) surface as unhandled middleware exceptions
  (HTTP 500); the library does not guard them by design.

## Pros and Cons of the Options

### Fixed GUID generation

Zero configuration, but forces an ID format on all consumers and cannot be disabled.

### Pluggable `ICorrelationIdGenerator` service

Full DI integration, but an extra registered service and lifetime decisions for something that is
pure function-like logic.

### Delegate on options

Minimal API surface, nullable for "disabled"; not DI-friendly.
