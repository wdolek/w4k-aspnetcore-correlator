# 2. Flow correlation context via `AsyncLocal` ambient state

- Status: Accepted
- Date: 2026-09-08
- Tags: `correlation`, `context`, `async`, `architecture`

## Context and Problem Statement

Once the middleware reads/generates a correlation ID for the current request, application code and
outgoing HTTP calls made by that request must be able to access the value. The context must flow
across `await` boundaries, and it must be accessible from components that do not receive
`HttpContext` (most notably `CorrelatorHttpMessageHandler` running inside `HttpClient`).

## Decision Drivers

- Value must be accessible without passing `HttpContext` around.
- Value must flow across arbitrary async/await chains, including user-spawned tasks.
- Per-request isolation under high concurrency (no cross-request leakage).

## Considered Options

- Store in `HttpContext.Items` and expose via accessor
- Store in a custom awaitable context type (like `IHttpContextAccessor`)
- Store in `AsyncLocal<CorrelationContext>` (ambient, `ExecutionContext`-flowed)

## Decision Outcome

Chosen option: "Store in `AsyncLocal<CorrelationContext>`".

`CorrelationContextContainer` (a singleton implementing `ICorrelationContextFactory` accessor and
scope factory interfaces) holds a `static AsyncLocal<CorrelationContext?>`. The middleware creates a
scope (`CreateScope`), which sets the value, and `Dispose` clears it when the request completes.
`ICorrelationContextAccessor.CorrelationContext` reads the ambient value, falling back to
`EmptyCorrelationContext.Instance` when no scope is active.

### Consequences

- **Positive:** correlation flows anywhere `ExecutionContext` flows - background tasks, queued work
  items, and `DelegatingHandler`s in outgoing `HttpClient` pipelines.
- **Positive:** no dependency on `IHttpContextAccessor` or ASP.NET types at the point of consumption.
- **Positive:** per-request isolation is guaranteed by `AsyncLocal` semantics; covered by
  `ConcurrentCorrelatorTests`.
- **Negative:** `AsyncLocal` writes copy the `ExecutionContext`, causing allocations invisible to
  the library's users; a measurable (small) hot-path cost, tracked in benchmarks.
- **Negative:** ambient state can surprise users who fork execution flow - the correlation context
  "follows" child tasks, which may or may not be desired (e.g. fire-and-forget work).
- **Negative:** consumers doing fire-and-forget work that outlives the request may see the scope
  disposed underneath them (accessor then returns `EmptyCorrelationContext`).

## Pros and Cons of the Options

### Store in `HttpContext.Items`

Cheap and request-scoped, but unusable from `CorrelatorHttpMessageHandler` without threading
`HttpContext` or `IHttpContextAccessor` into HTTP client pipelines.

### Store in a custom awaitable context type

Would require consumers to adopt a library-specific pattern for every async call - hostile to
usability for a cross-cutting concern.

### Store in `AsyncLocal<CorrelationContext>`

Ambient, flows with `ExecutionContext`, works in `DelegatingHandler`s. Slight allocation overhead
and the usual ambient-state caveats.
