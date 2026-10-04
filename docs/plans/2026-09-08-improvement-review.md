# Improvement review - 2026-09-08

Review of `W4k.AspNetCore.Correlator` (originally reviewed at v3.4.0) focused on security,
performance, API design and usability. Findings reference the [ADRs](../adr/README.md) where
relevant and include concrete code locations.

> Revised: 2026-09-09 - security items reclassified per non-breaking policy, TFM/CI finding closed,
> U1/U4/U5 status recorded.
>
> Re-reviewed: 2026-09-13 - statuses refreshed against current master: U1/U4/U5 merged
> (#73/#74/#75), S1 merged (#76) and released as 3.5.0 (StyleCop.Analyzers removed in the same
> release prep; CI/pack workflows fixed in #77-#79), S2/S3 implemented on branches and rebased
> onto master after 3.5.0 (3.5.0 moved all `PublicAPI.Unshipped.txt` entries to `Shipped` - the
> branches' additions were reconciled accordingly). All other findings re-verified as still
> applicable to current code.

Guiding constraints (deliberate design decisions - not defects):

- **High-availability APIs:** no unconditional per-request cost may be added. Validation is opt-in
  by design; trusted-network (intranet) deployments pay nothing for validation they don't need.
- **Minimal dependency footprint:** no `Microsoft.Extensions.Hosting` dependency; options
  validation is lazy (`Validate(...)` only, no `ValidateOnStart`).
- **Non-breaking policy (2026-09-09):** fixes must not change the library's default behavior.
  Anything that would change behavior ships as an **opt-in feature** (new option, new registration,
  new component) with the current behavior as default - or is deferred to the next major version.
- **Runtime lifecycle:** net8.0/net9.0 are near (or past) end of support; no effort is spent
  broadening their coverage (see R1 - planned TFM removal instead).

## Summary

| #  | Finding | Area | Priority | Effort | Status |
|----|---------|------|----------|--------|--------|
| S1 | No security guidance for internet-facing deployments; opt-in validation deserves a discoverable one-liner helper | Docs/Security | Medium | S | **Merged** (#76), shipped in 3.5.0 |
| S2 | Invalid correlation neither rejected nor regenerated - request continues without any correlation; **opt-in policy** `OnInvalid` proposed | Security/UX | High | M | **Implemented** - `feature/s2-invalid-correlation-policy` (rebased, ready to merge) |
| S3 | Header names in options are not validated - potential break for invalid configs, needs decision (see item) | Security/UX | Medium | S | **Implemented** - `feature/s3-header-name-validation` (rebased, ready to merge) |
| S4 | Invalid header value reaching response emission fails as late 500 - behavior-preserving options only | Security | Medium | S | Open (optional feature) |
| S5 | `ReplaceTraceIdentifier` injects unsanitized value into platform trace logs - docs-first; sanitize only as opt-in | Security | Medium | S | Open |
| P1 | Per-request closure allocation in `OnStarting` registration | Performance | Low | S | Open |
| P2 | `CorrelatedLoggerState` allocations per request when logging scope enabled | Performance | Low | S | Open |
| P3 | `CorrelationId` equality is `OrdinalIgnoreCase` - behavior change, next major | API/Perf | Medium | S | Open (next major) |
| P4 | `CorrelatorHttpMessageHandler` overrides only `SendAsync` - sync `HttpClient.Send` path silently skips forwarding | API/Correctness | High | S | Open |
| A1 | No type-based `WithValidator<T>()` overload (inconsistent with factory/emitter registration) | API | Low | S | Open |
| A2 | Single-validator limit - no built-in composite | API | Low | M | Open |
| A3 | No charset/format validator shipped despite having a safe-char allowlist internally | API/Security | Medium | S | **Done** - `CorrelationValuePatternValidator` shipped in #76 |
| A4 | `ICorrelationContextAccessor` silently returns `EmptyCorrelationContext` when middleware not used | API/UX | Medium | S | Open (next major) |
| A5 | Options not bindable from `IConfiguration` - documented pattern only, per ADR 0012 | Usability | Low | M | Open |
| A6 | `Microsoft.Extensions.Options.DataAnnotations` referenced but unused - violates minimal-footprint decision (ADR 0012) | Maintenance | Medium | S | Open |
| A7 | `default(PropagationSettings)` silently behaves as `PropagateAs("X-Correlation-Id")` instead of `NoPropagation` | API | Medium | S | Open |
| U1 | `docs/configuration.md` sample won't compile | Docs | High | S | **Merged** (#73) |
| U4 | CORS consumers need `WithExposedHeaders` - not documented | Docs | Low | S | **Merged** (#74) |
| U5 | Minor items: `PackageIconUrl`, XML doc typo, `Request-Id` claim, CHANGELOG | Docs | Low | S | **Merged** (#75) |
| T1 | No integration tests for sync `HttpClient.Send` forwarding (would currently fail, see P4) and for `Emit` + invalid-value paths | Testing | High | S | Open |
| T2 | ~~CI: integration tests net10.0 only, Linux only~~ - won't fix: net8.0/net9.0 near EOL, older runtimes to be dropped (R1) | Testing | - | - | **Closed - won't fix** |
| R1 | Drop net8.0/net9.0 TFMs once out of support; simplifies csproj conditionals, sanitizer dual implementation, test matrix | Maintenance | Low | M | Open (post-EOL, e.g. net8 EOL 2026-11) |

---

## Security

Design context (ADR 0004): validation is deliberately opt-in. Trusted-network deployments - the
library's primary audience - get a validation-free hot path; log injection via correlator's own
output is already closed unconditionally by sanitization (ADR 0007).

**Policy (2026-09-09):** skip anything that changes default behavior; implement as opt-in feature
where valuable. Each item below states its breaking-change classification.

### S1. Security guidance and discoverable opt-in helpers for internet-facing deployments - MERGED (#76)

**Implemented and shipped in 3.5.0:** README "Security" section, `WithDefaultValidator()` one-liner
and `CorrelationValuePatternValidator` (also covers A3). Kept below for context.

**Non-breaking** (docs + new opt-in APIs).

For internet-facing APIs the opt-in validator is strongly advisable, but today it requires knowing
`CorrelationValueLengthValidator` exists. README/docs contain no security section at all.

**Recommendation:**

- Add a short "Security" section to README + `docs/configuration.md`: if the API is reachable by
  untrusted parties, register a validator (length and/or charset) - and combine
  `ReplaceTraceIdentifier` with validation (see S5).
- Ship a discoverable opt-in convenience: `.WithDefaultValidator()` (length-bounded) and a
  charset validator (A3), so hardening is a single line instead of a custom implementation.
  Zero cost for consumers who don't call it - the opt-in default is unchanged (ADR 0004).

### S2. Invalid value: opt-in policy for what happens after failed validation

**Implemented on `feature/s2-invalid-correlation-policy`** (`OnInvalid` policy with `KeepEmpty`/
`GenerateNew`/`Reject`, `RegeneratedCorrelationContext`, middleware 400, ADR 0013 on the
docs/adrs-and-plans branch, unit + integration tests) - rebased onto master after 3.5.0 (PublicAPI reconciled), ready to merge. Kept
below for context.

**Non-breaking as opt-in feature.** New `CorrelationOptions` property, default preserving current
behavior; new behavior only when explicitly configured (and only meaningful when a validator is
registered - zero cost otherwise).

When validation fails, `InvalidCorrelationContext` is created with `CorrelationId.Empty`
(`CorrelationContextFactory.cs:47-49`). The request then proceeds **without any correlation ID** -
the generated-ID factory is not consulted for the fallback. A request with a bogus header ends up
uncorrelated in logs and for downstream calls, which harms traceability exactly for (potentially
malicious) traffic.

**Recommendation:**

```csharp
// default: InvalidCorrelationPolicy.KeepEmpty (current behavior, unchanged)
options.OnInvalid = InvalidCorrelationPolicy.GenerateNew; // opt-in
// InvalidCorrelationPolicy.Reject (400) - optional, for hardened deployments
```

Requires a new context type or a `GeneratedCorrelationContext` variant carrying the original
header name and validation result. Amends ADR 0004 - needs its own ADR before implementation.

### S3. Header names are not validated - decision needed (potential break)

**Implemented on `feature/s3-header-name-validation`** per the recommendation below: strict
validation in `PropagateAs`/`KeepIncomingHeaderName` factories + opt-in `ValidateHeaderNames`
knob for `ReadFrom` (default off, non-breaking), with tests - rebased onto master after 3.5.0
(PublicAPI reconciled), ready to merge. Kept below for context.

**Potentially breaking if done strictly** - today's behavior differs per location:

- `ReadFrom` entries are used verbatim in `IHeaderDictionary.TryGetValue` - an invalid header name
  (e.g. `"X CID"` or `""` empty string) silently never matches. Strict validation would make such
  configs fail at startup: a **behavior change** for garbage-but-harmless configurations.
- `PropagationSettings.PropagateAs(name)` accepts any non-empty string; an invalid name already
  fails today - but *late* (emitter `OnStarting` -> 500, or `HttpRequestHeaders.Add` throw
  mid-request). Failing at registration time converts a guaranteed runtime failure into a startup
  failure - same class of failure, earlier.

**Recommendation (opt-in / fail-fast on already-broken paths only):**

- `PropagateAs`/`KeepIncomingHeaderName` factories: validate header name (RFC 7230 token charset)
  at construction - converts existing guaranteed request-time failure into registration-time
  failure. Recommend accepting this as a bug-fix-grade change, maintainer decision required.
- `ReadFrom`: strict validation would be a breaking change -> do **not** throw by default.
  Options: (a) validation only via an opt-in knob, or (b) no library change, covered by S1
  guidance. Recommend (a) or leave as docs note - needs decision.

### S4. Invalid *value* reaching response emission fails late - optional feature only

**Behavior change if implemented as unconditional skip** (today: 500 at response start; after:
header silently skipped) - so not eligible as a default-path fix under the non-breaking policy.

`AddHeaderIfNotSet` (`HeaderDictionaryExtensions.cs:16`) `TryAdd`s the value; Kestrel rejects
values containing invalid characters (e.g. CR/LF) only when the response actually starts - the
`OnStarting` callback throwing produces a 500 with no useful diagnostics.

**Recommendation:**

- Primary mitigation is already opt-in: register a validator on untrusted boundaries (S1/S2).
- Optional feature (needs decision): skip response emission for values containing control
  characters - either unconditionally (arguably a bug fix of an attacker-triggered crash path,
  but technically a behavior change) or as an opt-in emitter setting. If implemented, pair with a
  test documenting the new behavior and an ADR-0008 amendment.

### S5. `ReplaceTraceIdentifier` + unsanitized value - docs-first, sanitize only as opt-in

**Behavior change if sanitized unconditionally** (the assigned `TraceIdentifier` would differ
from the raw correlation value) - so docs-first.

`CorrelatorMiddleware.Invoke` (`CorrelatorMiddleware.cs:58-62`) assigns the raw correlation value
to `TraceIdentifier`, which then appears in ASP.NET's own request logs / W3C logs / `Activity`
diagnostics - outside the reach of `CorrelationIdValueSanitizer` (which protects only correlator's
log messages, ADR 0007).

**Recommendation:**

- Docs (non-breaking): document in README/configuration that `ReplaceTraceIdentifier` should be
  combined with a validator when the API is reachable by untrusted parties; include in the S1
  security section.
- Optional feature (needs decision + ADR-0003 amendment): `options.SanitizeTraceIdentifier`
  (default `false`), sanitizing before assignment - cost only for opted-in requests with unsafe
  values, same fast-path profile as the existing sanitizer.

---

## Performance

Context: hot path is already lean (cached `LoggerMessage.Define` delegates, `SearchValues`-based
sanitizer with fast path, `IsEnabled` guards, span-based header lookup, benchmarks in repo) - in
line with the high-availability goal. The findings below are micro-optimizations; validate against
`benchmarks/` before/after.

### P1. Closure allocation per request for `OnStarting`

`CorrelatorMiddleware.Invoke` (`CorrelatorMiddleware.cs:51`) captures `httpContext` and
`correlationContext` in a lambda (plus the `_emitter` state box) for every request with `Emit`
enabled. A static callback + small state tuple (emitter + context + httpContext) avoids the
delegate/closure allocation:

```csharp
httpContext.Response.OnStarting(static state =>
{
    var s = (EmitState)state;
    return s.Emitter.Emit(s.HttpContext, s.Context);
}, new EmitState(_emitter, httpContext, correlationContext));
```

(Still allocates the state object, but one object instead of closure + display class; or pool it.)
Benchmark before adopting - expected win is small but the path runs on *every* request with `Emit`
enabled.

### P2. `CorrelatedLoggerState` allocations

`CorrelatedLoggerState` (`CorrelatedLoggerState.cs:8-13`) allocates a `List<KeyValuePair<...>>`
per request when logging scope is enabled. A single-element array (or a struct implementing
`IReadOnlyList<KeyValuePair<string, object>>`) plus a precomputed `ToString` removes one
allocation. Same caveat: benchmark first.

### P3. `CorrelationId` equality semantics - next major

`CorrelationId.Equals/GetHashCode` use `OrdinalIgnoreCase` (`CorrelationId.cs:77-89`). Header
*names* are case-insensitive in HTTP, but correlation *values* are opaque tokens - treating
`"abc"` and `"ABC"` as the same correlation is surprising, slightly slower, and can cause false
deduplication (e.g. grouping log entries). **Behavior change** - defer to the next major version;
document the current semantics meanwhile.

---

## API design

### P4. `CorrelatorHttpMessageHandler` ignores synchronous `HttpClient.Send`

The handler overrides only `SendAsync` (`CorrelatorHttpMessageHandler.cs:37`). Since .NET 5,
`HttpClient.Send`/sync paths call `HttpMessageHandler.Send`, which the base `DelegatingHandler`
forwards without correlation headers. Consumers using sync APIs (or libraries doing so internally)
silently lose correlation forwarding - for a correlation library this is a correctness bug, not
just an optimization miss. **Non-breaking** (fixes a silent no-op path).

**Recommendation:** override `protected override HttpResponseMessage Send(HttpRequestMessage,
CancellationToken)` with the same forwarding logic and test both paths. High value, trivial fix, no
API change.

### A1. Missing `WithValidator<T>()` overload

Factory and emitter have type-based registration (`WithCorrelationContextFactory<T>` /
`WithCorrelationEmitter<T>`), validator only instance-based (`WithValidator<T>(T validator)`,
`CorrelatorBuilderExtensions.cs:105`). Add `WithValidator<T>() where T : class,
ICorrelationValidator` for consistency (and `WithDefaultValidator()` when S1 introduces one).
**Non-breaking** (new overloads).

### A2. Single-validator limit without built-in composite

ADR 0009 acknowledges consumers must hand-roll composite validation. Common ask will be
"length + charset". Provide a built-in composite:

```csharp
builder.WithValidators(new CorrelationValueLengthValidator(64), new CorrelationValuePatternValidator());
```

or at minimum a public `CompositeCorrelationValidator` they can assemble themselves. All opt-in;
unchanged default path. **Non-breaking.**

### A3. Ship a charset/pattern validator - DONE (#76)

**Shipped in 3.5.0 as `CorrelationValuePatternValidator`** (allowlist-based, length-bounded,
reusing the sanitizer's safe-char set) together with `.WithDefaultValidator()`. Kept below for
context.

The library already maintains a safe-char allowlist for logging
(`CorrelationIdValueSanitizer.SafeCorrelationIdCharsString`). Expose a
`CorrelationValuePatternValidator` (allowlist-based, optionally length-bounded) reusing the same
char set. Cheap to implement, directly serves S1's guidance (hardening one-liner for
internet-facing APIs), and pairs naturally with the sanitizer. **Non-breaking** (new opt-in type).

### A4. Accessor masks misconfiguration - next major

`ICorrelationContextAccessor.CorrelationContext` returns `EmptyCorrelationContext.Instance` both
for "no correlation received/generated" and "middleware never used / accessed outside request"
(`CorrelationContextContainer.cs:20`). A consumer with a DI misconfiguration silently gets empty
IDs everywhere. Changing the returned contract is a **behavior change** - next major. In the
meantime: document the fallback in `ICorrelationContextAccessor` XML docs (**non-breaking**).

### A5. Options not bindable from `IConfiguration`

`CorrelatorOptions` mixes a delegate (`Factory`), readonly-struct settings and a get-only
`List<string>` (`CorrelatorOptions.cs`) - only `ReadFrom` is partially binder-friendly. Full
binding support would add a parsing/binding subsystem the library deliberately avoids (ADR 0012).

**Recommendation:** document a consumer-side pattern (bind a POCO, map to `CorrelatorOptions` in
the configure delegate). Keep `Factory` code-only by design.

### A6. Unused package reference contradicts the minimal-footprint decision

`Microsoft.Extensions.Options.DataAnnotations` is referenced for all TFMs
(`W4k.AspNetCore.Correlator.csproj:74,80,86`) but no data annotations are used. Every consumer
takes this transitive dependency for nothing - directly at odds with ADR 0012.

**Recommendation:** remove the reference from all TFMs (options rules stay as plain
`Validate(...)` callbacks). Pure win: smaller dependency graph, better alignment with the stated
principle. **Non-breaking** (dependency-graph only; assemblies unchanged).

### A7. `default(PropagationSettings)` trap

`HeaderName`'s null-fallback makes `default(PropagationSettings)` behave as
`PropagateAs("X-Correlation-Id")` rather than `NoPropagation` (`PropagationSettings.cs:43`) -
`Settings` defaults to 0 (`NoPropagation`) but `HeaderName` "recovers", so the enum check saves it
today. Still, the `HeaderName ?? HttpHeaders.CorrelationId` fallback is a footgun for future
maintainers (it exists only because of the `default` struct case). Consider an internal
`IsDefault` check or documenting it (ADR 0005 already flags it). **Non-breaking** (internal/doc).

---

## Usability & docs

> U1, U4 and U5 were implemented on per-fix branches on 2026-09-09 and merged as #73/#74/#75.
> Status recorded for traceability; details kept for context.

### U1. `docs/configuration.md` sample does not compile - MERGED (#73)

Merged: invalid lambda syntax in the "Adjusting default
configuration" sample (`ReplaceTraceIdentifier = true,` inside a lambda body) and nonexistent
`correlatorOptions.Add(...)` method - corrected to `ReadFrom.Add(...)`.

### U4. CORS note missing - MERGED (#74)

Merged: "Exposing emitted header to browsers (CORS)" section added to
`docs/configuration.md` with a `WithExposedHeaders` sample, including the clarification that CORS
concerns browser clients only and is unrelated to machine-to-machine communication.

### U5. Minor docs/README items - MERGED (#75)

Merged: deprecated `PackageIconUrl` dropped from the csproj; "classs" XML
doc typo in `CorrelationContext.cs`; `Request-Id` claim corrected (legacy header injected by
`HttpClient` activity propagation on .NET Core 3.x - current .NET sends W3C `traceparent`);
`CHANGELOG.md` added (Keep-a-Changelog format) and linked from README.

---

## Testing & CI

### T1. Coverage gaps for current behavior (drive P4 fix)

Missing integration/unit tests:

- synchronous `HttpClient.Send` correlation forwarding (would currently **fail** - see P4),
- `Emit` + invalid correlation value (no response header, no 500),
- `Emit` when app pre-sets the correlation response header (first-wins semantics, ADR 0008),
- correlation value with CRLF reaching emitter without validator (documents current 500 behavior;
  see S4),
- fire-and-forget task accessing `ICorrelationContextAccessor` after scope disposal (documents
  `EmptyCorrelationContext` semantics, ADR 0002).

### T2. CI matrix / multi-TFM integration tests - CLOSED (won't fix)

Integration tests target `net10.0` only and CI runs `ubuntu-latest` only. **Decision (2026-09-09):**
not worth addressing - net9.0 is already out of support and net8.0 reaches EOL within months
(2026-11); broadening coverage for runtimes about to be dropped is wasted effort. Instead, older
TFMs will be removed outright (R1), after which the single-TFM story holds naturally.

### T3. Docs sample compilation

The broken configuration sample (U1) recurring suggests a small "docs snippets" test project
referenced from `docs/` samples, compiled in CI (docs paths currently ignored in CI, which is why
U1 went unnoticed). A tiny console project with the documented snippets as source would prevent
drift. Still relevant after U1 - guards future docs regressions.

---

## Maintenance & lifecycle

### R1. Drop net8.0/net9.0 TFMs once out of support

Per maintainer direction (2026-09-09): older runtime support will be removed soon. Trigger: after
net8.0 EOL (2026-11-10). Scope when executing:

- remove `net8.0`/`net9.0` from `TargetFrameworks` in src/tests/benchmarks csproj and drop the
  per-TFM conditional `PackageReference` ItemGroups (`W4k.AspNetCore.Correlator.csproj:71-87`),
- remove the net8 sanitizer implementation (`CorrelationIdValueSanitizer.cs`, `#if NET8_0`
  branch) - single implementation remains,
- drop the `RestoreEnablePackagePruning` workaround (`W4k.AspNetCore.Correlator.csproj:15`),
- update `global.json`/CI workflow .NET versions accordingly (`.github/workflows/ci.yml`),
- update ADR 0011 (supersede the multi-targeting decision) and add a CHANGELOG entry; minor or
  major version bump per SemVer policy for dropped framework support (normally a major bump -
  decide whether the soon-EOL status justifies calling it breaking).

---

## Suggested sequencing

1. **Landed:** U1/U4/U5 (#73/#74/#75), S1 (#76, 3.5.0). CI/pack fixes (#77-#79).
2. **Ready to merge:** S2 (`OnInvalid` policy + ADR 0013) and S3 (header name validation) - both
   implemented on branches and rebased onto master after 3.5.0 (PublicAPI additions reconciled
   with the release's Shipped/Unshipped move); tests green.
3. **Quick wins (no breaking changes, no new dependencies):** P4 (sync `Send` override) + T1
   tests; A6 (remove unused DataAnnotations reference); A1 (`WithValidator<T>()` overload).
4. **Hardening, strictly opt-in (minor-version friendly):** S5 docs part; A2 (composite
   validator).
5. **Opt-in features (need an ADR each before implementation):** S4 (emitter control-char skip,
   amends ADR 0008), S5 optional `SanitizeTraceIdentifier` (amends ADR 0003).
6. **Next major:** P3 (`Ordinal` equality), A4 (accessor semantics) - each with ADR updates.
7. **Post-EOL:** R1 (drop net8.0/net9.0, supersedes ADR 0011's multi-target wave).
8. **Ongoing:** P1/P2 micro-optimizations guarded by benchmarks; T3 docs-compile guard.
