# 9. Single-registration DI components with fail-fast guards

- Status: Accepted
- Date: 2026-09-08
- Tags: `di`, `api-design`, `registration`

## Context and Problem Statement

Correlator consists of a few replaceable components: `ICorrelationContextFactory`,
`ICorrelationEmitter` and `ICorrelationValidator`. Unlike typical DI registrations which use
last-wins/`TryAdd` semantics, the library must guarantee that exactly one implementation of each is
active - the middleware and handlers depend on unambiguous behavior.

## Decision Drivers

- Ambiguous component resolution must be impossible to miss.
- Component registration API should be fluent and discoverable (`ICorrelatorBuilder`).
- All components are stateless with request state in the ambient context -> singleton lifetime.

## Considered Options

- Standard `TryAdd`/last-wins registrations
- Single-registration builder methods that throw on duplicate registration
- Composite resolution (resolve `IEnumerable<T>`, wrap in composite)

## Decision Outcome

Chosen option: "Single-registration builder methods that throw on duplicate registration".

`WithCorrelationContextFactory<T>`, `WithCorrelationEmitter<T>` and `WithValidator<T>(instance)`
scan `IServiceCollection` for an existing registration of the service type and throw
`InvalidOperationException` (via `ThrowHelper`) on duplicates. All components register as
singletons. The fluent `ICorrelatorBuilder` (returned by `AddCorrelator`/`AddDefaultCorrelator`)
exposes `Services` for escape hatches.

### Consequences

- **Positive:** misconfiguration (double registration, e.g. `AddDefaultCorrelator` followed by
  `WithCorrelationContextFactory<T>`) fails loudly at startup instead of silently changing
  behavior.
- **Positive:** singletons keep the request path allocation-free.
- **Negative:** `Services.Any(...)` scans the whole collection per registration call - O(n) at
  startup only, acceptable.
- **Negative:** no multi-validator composition - consumers wanting composite validation must
  implement the composite themselves (documented); see improvement plan.
- **Negative:** duplicate detection is string-of-registrations based; it cannot detect replacements
  done directly on `IServiceCollection` after the builder methods ran.
