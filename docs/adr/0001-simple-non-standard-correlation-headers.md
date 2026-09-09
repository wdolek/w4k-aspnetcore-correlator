# 1. Use simple non-standard correlation headers, not W3C Trace Context

- Status: Accepted
- Date: 2026-09-08
- Tags: `correlation`, `http`, `protocol`, `scope`

## Context and Problem Statement

The library must let services correlate requests across HTTP boundaries. There are two families of
standards in this space:

1. Simple, non-standard single-value headers (`X-Correlation-Id`, `X-Request-Id`, `Request-Id`),
   widely used in enterprise and interop scenarios.
2. The [W3C Trace Context](https://www.w3.org/TR/trace-context/) specification (`traceparent`/`tracestate`),
   the basis of distributed tracing, with first-class support in .NET
   ([`System.Diagnostics.Activity`](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing),
   OpenTelemetry).

Implementing the library on top of either is possible; picking one defines the scope of the library
for its entire lifetime.

## Decision Drivers

- Library must stay small and focused ("middleware + message handler").
- Typical consumers already have non-standard headers in use and cannot change their contracts.
- .NET has built-in, battle-tested distributed tracing; re-implementing it would add no value.

## Considered Options

- Read/write simple non-standard correlation headers only
- Implement W3C Trace Context (subset)
- Support both, with a mode switch

## Decision Outcome

Chosen option: "Read/write simple non-standard correlation headers only".

Correlator reads a configurable, ordered list of headers (`X-Correlation-Id`, `X-Request-Id`,
`Request-Id` by default) and propagates a single correlation value. W3C Trace Context is explicitly
**not supported**, and this is stated in the README so consumers can make an informed choice.

### Consequences

- **Positive:** small, fast, zero dependency on tracing libraries; trivial to reason about.
- **Positive:** transparent for consumers with legacy header contracts (custom header names supported
  in both read and propagate directions).
- **Negative:** correlation is a single opaque value - no parent/child span semantics, no sampling.
- **Negative:** consumers needing real tracing must use .NET distributed tracing / OpenTelemetry
  alongside Correlator; the two correlation values are not unified.

## Pros and Cons of the Options

### Read/write simple non-standard correlation headers only

Simple, focused, covers the common "log correlation across hops" scenario; no span semantics.

### Implement W3C Trace Context (subset)

Interop with tracing-aware systems, but a partial implementation is a trap (header parsing rules,
versioning, tracestate limits) and competes with built-in `System.Diagnostics` support.

### Support both, with a mode switch

Maximal flexibility at the cost of doubled test matrix, doubled bug surface and confusing
configuration for a library whose goal is simplicity.
