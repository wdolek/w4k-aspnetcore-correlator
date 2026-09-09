# 5. Propagation modeled as mode + header name (`PropagationSettings`)

- Status: Accepted
- Date: 2026-09-08
- Tags: `api-design`, `options`, `propagation`

## Context and Problem Statement

Correlation ID must be propagated in two directions - emitted to the HTTP response and forwarded to
subsequent requests via `CorrelatorHttpMessageHandler`. For each direction, users must be able to:
disable propagation, propagate under a fixed header name, or propagate under the header name the
correlation was *received* with (with a fallback for generated IDs).

## Decision Drivers

- Express three modes per direction without nullable/boolean option soup.
- Immutable, comparable, easy-to-bind values that cannot represent invalid combinations.
- Consistent model for both `Emit` and `Forward`, overridable per HTTP client.

## Considered Options

- Separate options (`bool EmitEnabled`, `string EmitHeader`, `bool KeepIncoming...`)
- Enum `HeaderPropagation` alone (no header name)
- Readonly struct `PropagationSettings` combining `HeaderPropagation` mode + header name, created
  via factory methods

## Decision Outcome

Chosen option: "Readonly struct `PropagationSettings`".

`PropagationSettings` couples `HeaderPropagation` (enum: `NoPropagation`, `UsePredefinedHeaderName`,
`KeepIncomingHeaderName`) with a header name field, and can only be constructed through named
factories: `NoPropagation`, `PropagateAs(name)`, `KeepIncomingHeaderName(defaultName)`. The struct
implements equality and guards its factories against null/empty names. `CorrelatorOptions.Emit` and
`.Forward` are both of this type; `WithCorrelation(propagationSettings)` overrides `Forward` per
HTTP client without touching global options.

### Consequences

- **Positive:** invalid states (e.g. "keep incoming name" without a fallback name) are not
  constructible through the public API.
- **Positive:** one mental model for response emission and request forwarding.
- **Positive:** equality makes options comparison and testing straightforward.
- **Negative:** `default(PropagationSettings)` silently behaves as
  `UsePredefinedHeaderName` + `X-Correlation-Id` (via the `HeaderName` null-fallback) instead of
  `NoPropagation` - a subtle trap for consumers constructing the struct via `default`.
- **Negative:** mode dispatch happens via switch on `(Settings, context type)` tuples in emitter
  and handler; adding a mode touches several places.
