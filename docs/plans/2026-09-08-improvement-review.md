# Improvement review - 2026-09-08

Review of the current solution (`W4k.AspNetCore.Correlator` v3.4.0) focused on security,
performance, API design and usability. Findings reference the [ADRs](../adr/README.md) where
relevant and include concrete code locations.

Guiding constraints (per ADR 0004 and ADR 0012, deliberate design decisions - not defects):

- **High-availability APIs:** no unconditional per-request cost may be added. Validation is opt-in
  by design; trusted-network (intranet) deployments pay nothing for validation they don't need.
- **Minimal dependency footprint:** no `Microsoft.Extensions.Hosting` dependency; options
  validation is lazy (`Validate(...)` only, no `ValidateOnStart`).

All proposals below respect these constraints: anything runtime-priced is opt-in, anything
startup-priced happens at registration time, and no proposal adds a package dependency.

## Summary

| #  | Finding | Area | Priority | Effort |
|----|---------|------|----------|--------|
| S1 | No security guidance for internet-facing deployments; opt-in validation deserves a discoverable one-liner helper | Docs/Security | Medium | S |
| S2 | Invalid correlation neither rejected nor regenerated - request continues without any correlation (opt-in policy proposed) | Security/UX | High | M |
| S3 | Header names in options are not validated at registration time (garbage names fail silently or as late 500s) | Security/UX | Medium | S |
| S4 | Invalid header value reaching response emission fails as late 500 (server-side header validation) | Security | Medium | S |
| S5 | `ReplaceTraceIdentifier` injects unsanitized value into ASP.NET/platform trace logs | Security | Medium | S |
| S6 | Lazy options validation is deliberate (ADR 0012) - but undocumented; consumers should know to call `ValidateOnStart()` themselves | Docs | Low | S |
| P1 | Per-request closure allocation in `OnStarting` registration | Performance | Low | S |
| P2 | `CorrelatedLoggerState` allocations per request when logging scope enabled | Performance | Low | S |
| P3 | `CorrelationId` equality is `OrdinalIgnoreCase` - subtle correctness + slower | API/Perf | Medium | S |
| P4 | `CorrelatorHttpMessageHandler` overrides only `SendAsync` - synchronous `HttpClient.Send` path silently skips correlation forwarding | API/Correctness | High | S |
| A1 | No type-based `WithValidator<T>()` overload (inconsistent with factory/emitter registration) | API | Low | S |
| A2 | Single-validator limit - no built-in composite | API | Low | M |
| A3 | No charset/format validator shipped despite having a safe-char allowlist internally | API/Security | Medium | S |
| A4 | `ICorrelationContextAccessor` silently returns `EmptyCorrelationContext` when middleware not used - misconfiguration is invisible | API/UX | Medium | S |
| A5 | Options not bindable from `IConfiguration` (delegates, structs, get-only list) - documented pattern only, per ADR 0012 | Usability | Low | M |
| A6 | `Microsoft.Extensions.Options.DataAnnotations` referenced but unused - violates the minimal-footprint decision (ADR 0012) | Maintenance | Medium | S |
| A7 | `default(PropagationSettings)` silently behaves as `PropagateAs("X-Correlation-Id")` instead of `NoPropagation` | API | Medium | S |
| U1 | `docs/configuration.md` sample won't compile (invalid lambda syntax, nonexistent `Add` method) | Docs | High | S |
| U2 | No documented EventId catalog for consumers filtering correlator log output | Docs | Low | S |
| U3 | `TestResults/`, `BenchmarkDotNet.Artifacts/` and `.idea/` artifacts committed to the repo | Maintenance | Low | S |
| U4 | CORS consumers need `WithExposedHeaders` - not documented | Docs | Low | S |
| T1 | No integration tests for sync `HttpClient.Send` forwarding (would currently fail, see P4) and for `Emit` + invalid-value paths | Testing | High | S |
| T2 | CI: integration tests run `net10.0` only, Linux only (see ADR 0011) | Testing | Low | M |

---

## Security

Design context (ADR 0004): validation is deliberately opt-in. Trusted-network deployments - the
library's primary audience - get a validation-free hot path; log injection via correlator's own
output is already closed unconditionally by sanitization (ADR 0007). The findings below are about
making the *opt-in* path easy, safe-by-configuration and well-guided - not about changing the
default.

### S1. Security guidance and discoverable opt-in helpers for internet-facing deployments

For internet-facing APIs the opt-in validator is strongly advisable, but today it requires knowing
`CorrelationValueLengthValidator` exists. README/docs contain no security section at all.

**Recommendation:**

- Add a short "Security" section to README + `docs/configuration.md`: if the API is reachable by
  untrusted parties, register a validator (length and/or charset) - and combine
  `ReplaceTraceIdentifier` with validation (see S5).
- Ship a discoverable opt-in convenience: `.WithDefaultValidator()` (length-bounded) and a
  charset validator (A3), so hardening is a single line instead of a custom implementation.
  Zero cost for consumers who don't call it - the opt-in default is unchanged (ADR 0004).

### S2. Invalid value: neither rejected nor regenerated (opt-in policy)

When validation fails, `InvalidCorrelationContext` is created with `CorrelationId.Empty`
(`CorrelationContextFactory.cs:47-49`). The request then proceeds **without any correlation ID** -
the generated-ID factory is not consulted for the fallback. A request with a bogus header ends up
uncorrelated in logs and for downstream calls, which harms traceability exactly for (potentially
malicious) traffic.

**Recommendation:** an *opt-in* invalid-value policy, meaningful only when a validator is
registered (zero cost otherwise, consistent with ADR 0004):

```csharp
options.OnInvalid = InvalidCorrelationPolicy.GenerateNew; // default: KeepEmpty (current behavior)
```

Optionally also `Reject` (400) for hardened deployments that want an explicit contract. Requires a
new context type or a `GeneratedCorrelationContext` variant carrying the original header name and
validation result. Amends ADR 0004 (new behavior branch), so it should get its own ADR before
implementation.

### S3. Header names are not validated anywhere

- `ReadFrom` entries are used verbatim in `IHeaderDictionary.TryGetValue` - an invalid header name
  (e.g. `"X CID"` or `""` empty string) simply never matches, failing silently.
- `PropagationSettings.PropagateAs(name)` accepts any non-empty string; an invalid name surfaces
  only when the emitter runs (`OnStarting` -> late 500) or when the message handler adds it
  (`HttpRequestHeaders.Add` -> throw mid-request).

**Recommendation:** validate header names (RFC 7230 token charset) at **registration time** - in
the options `Validate(...)` callback in `AddCorrelator` (`ServiceCollectionExtensions.cs:69`) and in
`PropagationSettings.PropagateAs`/`KeepIncomingHeaderName` factories. Startup-time cost only, no
per-request overhead, no new dependency (ADR 0012).

### S4. Invalid *value* reaching response emission fails late

`AddHeaderIfNotSet` (`HeaderDictionaryExtensions.cs:16`) `TryAdd`s the value; Kestrel rejects
values containing invalid characters (e.g. CR/LF) only when the response actually starts - the
`OnStarting` callback throwing produces a 500 with no useful diagnostics.

**Recommendation:** covered by S1/S2 guidance (register a validator on untrusted boundaries) and
S3; additionally, the default `CorrelationEmitter` could defensively skip values containing
control characters when writing the response header - a cheap scan of a typically short value on
the emit path only (skipped entirely when `Emit` is disabled), consistent with the HA goal.

### S5. `ReplaceTraceIdentifier` + unsanitized value

`CorrelatorMiddleware.Invoke` (`CorrelatorMiddleware.cs:58-62`) assigns the raw correlation value to
`TraceIdentifier`, which then appears in ASP.NET's own request logs / W3C logs / `Activity`
diagnostics - outside the reach of `CorrelationIdValueSanitizer` (which protects only correlator's
log messages, ADR 0007).

**Recommendation:** either sanitize the value before assigning it as `TraceIdentifier` (cost only
when `ReplaceTraceIdentifier` is enabled and value contains unsafe chars - same profile as the
existing sanitizer fast path), or document prominently that `ReplaceTraceIdentifier` should be
combined with a validator on untrusted boundaries. Low implementation cost, removes a whole class
of log-injection complaints.

### S6. Lazy options validation - document it, don't change it

`AddCorrelator` calls `.Validate(options => options.ReadFrom.Count > 0, ...)` but never
`ValidateOnStart()` (`ServiceCollectionExtensions.cs:66-71`): eager validation would require the
hosting/startup-validator machinery, i.e. an extra dependency - deliberately rejected (ADR 0012).
An empty `ReadFrom` therefore surfaces when options are first resolved, not at application start.

**Recommendation (docs only):** document in `docs/registration.md` that options validation is
lazy by design, and that consumers using hosting can call `.ValidateOnStart()` on the options
builder themselves if they want startup fail-fast - their app already has the dependency, the
library doesn't.

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

### P3. `CorrelationId` equality semantics

`CorrelationId.Equals/GetHashCode` use `OrdinalIgnoreCase` (`CorrelationId.cs:77-89`). Header
*names* are case-insensitive in HTTP, but correlation *values* are opaque tokens - treating
`"abc"` and `"ABC"` as the same correlation is surprising, slightly slower, and can cause false
deduplication (e.g. grouping log entries).

**Recommendation:** switch to `Ordinal` in the next major (behavior change); document the current
semantics meanwhile.

---

## API design

### P4. `CorrelatorHttpMessageHandler` ignores synchronous `HttpClient.Send`

The handler overrides only `SendAsync` (`CorrelatorHttpMessageHandler.cs:37`). Since .NET 5,
`HttpClient.Send`/sync paths call `HttpMessageHandler.Send`, which the base `DelegatingHandler`
forwards without correlation headers. Consumers using sync APIs (or libraries doing so internally)
silently lose correlation forwarding - for a correlation library this is a correctness bug, not
just an optimization miss.

**Recommendation:** override `protected override HttpResponseMessage Send(HttpRequestMessage,
CancellationToken)` with the same forwarding logic and test both paths. High value, trivial fix, no
API change.

### A1. Missing `WithValidator<T>()` overload

Factory and emitter have type-based registration (`WithCorrelationContextFactory<T>` /
`WithCorrelationEmitter<T>`), validator only instance-based (`WithValidator<T>(T validator)`,
`CorrelatorBuilderExtensions.cs:105`). Add `WithValidator<T>() where T : class,
ICorrelationValidator` for consistency (and `WithDefaultValidator()` when S1 introduces one).

### A2. Single-validator limit without built-in composite

ADR 0009 acknowledges consumers must hand-roll composite validation. Common ask will be
"length + charset". Provide a built-in composite:

```csharp
builder.WithValidators(new CorrelationValueLengthValidator(64), new CorrelationValuePatternValidator());
```

or at minimum a public `CompositeCorrelationValidator` they can assemble themselves. All opt-in;
unchanged default path.

### A3. Ship a charset/pattern validator

The library already maintains a safe-char allowlist for logging
(`CorrelationIdValueSanitizer.SafeCorrelationIdCharsString`). Expose a
`CorrelationValuePatternValidator` (allowlist-based, optionally length-bounded) reusing the same
char set. Cheap to implement, directly serves S1's guidance (hardening one-liner for
internet-facing APIs), and pairs naturally with the sanitizer.

### A4. Accessor masks misconfiguration

`ICorrelationContextAccessor.CorrelationContext` returns `EmptyCorrelationContext.Instance` both
for "no correlation received/generated" and "middleware never used / accessed outside request"
(`CorrelationContextContainer.cs:20`). A consumer with a DI misconfiguration silently gets empty
IDs everywhere.

**Recommendation (vNext):** consider exposing `bool HasContext` (or making the ambient value
nullable internally while keeping the public contract); at minimum, document the fallback in
`ICorrelationContextAccessor` XML docs. Zero runtime cost either way.

### A5. Options not bindable from `IConfiguration`

`CorrelatorOptions` mixes a delegate (`Factory`), readonly-struct settings and a get-only
`List<string>` (`CorrelatorOptions.cs`) - only `ReadFrom` is partially binder-friendly. Full
binding support would add a parsing/binding subsystem the library deliberately avoids (ADR 0012).

**Recommendation:** document a consumer-side pattern (bind a POCO, map to `CorrelatorOptions` in
the configure delegate; or `AddCorrelator().BindConfiguration(...)` helper left as an exercise).
Keep `Factory` code-only by design.

### A6. Unused package reference contradicts the minimal-footprint decision

`Microsoft.Extensions.Options.DataAnnotations` is referenced for all TFMs
(`W4k.AspNetCore.Correlator.csproj:74,80,86`) but no data annotations are used. Every consumer
takes this transitive dependency for nothing - directly at odds with ADR 0012.

**Recommendation:** remove the reference from all TFMs (options rules stay as plain
`Validate(...)` callbacks). Pure win: smaller dependency graph, better alignment with the stated
principle.

### A7. `default(PropagationSettings)` trap

`HeaderName`'s null-fallback makes `default(PropagationSettings)` behave as
`PropagateAs("X-Correlation-Id")` rather than `NoPropagation` (`PropagationSettings.cs:43`) -
`Settings` defaults to 0 (`NoPropagation`) but `HeaderName` "recovers", so the enum check saves it
today. Still, the `HeaderName ?? HttpHeaders.CorrelationId` fallback is a footgun for future
maintainers (it exists only because of the `default` struct case). Consider an internal
`IsDefault` check or documenting it (ADR 0005 already flags it).

---

## Usability & docs

### U1. `docs/configuration.md` sample does not compile

Lines 20-46 mix statement syntax with member-declaration syntax inside a lambda:

```csharp
builder.Services.AddDefaultCorrelator(
    correlatorOptions =>
    {
        ...
        ReplaceTraceIdentifier = true,          // <- invalid in a lambda body
        LoggingScope = LoggingScopeSettings.IncludeLoggingScope(),
    });
```

Also `correlatorOptions.Add("X-Yet-Another-Request-ID")` (lines 76-80) - `CorrelatorOptions` has
no `Add` method; it must be `correlatorOptions.ReadFrom.Add(...)`. These are the docs most users
copy first; fix both and consider a docs-compile test (see T3).

### U2. Undocumented EventIds

Correlator logs use EventIds 101-103 (middleware), 202-206 (context factory), 301 (emitter)
(`Logging/LoggerExtensions.*.cs`). Consumers filtering or alerting on correlator logs need the
catalog. Add a table to `docs/components.md` or a new `docs/logging.md`.

### U3. Committed build artifacts

`TestResults/`, `BenchmarkDotNet.Artifacts/` (including multi-MB reports) and Rider's `.idea/`
workspace files are checked in. Add to `.gitignore` and remove from the repo (`.idea` at least the
`workspace.xml`). Also `W4k.AspNetCore.Correlator.sln.DotSettings.user` should be ignored.

### U4. CORS note missing

When `Emit` is enabled, browsers cannot read the correlation header unless the API exposes it
(`CorsPolicyBuilder.WithExposedHeaders(...)`). Document in `docs/configuration.md` - a very common
consumer complaint with custom response headers.

### U5. Minor docs/README items

- `PackageIconUrl` is deprecated; `PackageIcon` already set - drop the URL variant
  (`W4k.AspNetCore.Correlator.csproj:47`).
- `CorrelationContext` XML doc typo: "classs" (`CorrelationContext.cs:6`).
- README mentions `Request-Id` "(set by ASP.NET when sending request with `HttpClient`)" - worth
  verifying the statement still applies for current target frameworks and referencing docs.
- Consider a CHANGELOG (releases currently only visible via GitHub releases / commit log).

---

## Testing & CI

### T1. Coverage gaps for current behavior (drive P4/S2 fixes)

Missing integration/unit tests:

- synchronous `HttpClient.Send` correlation forwarding (would currently **fail** - see P4),
- `Emit` + invalid correlation value (no response header, no 500),
- `Emit` when app pre-sets the correlation response header (first-wins semantics, ADR 0008),
- correlation value with CRLF reaching emitter without validator (documents current 500 behavior;
  mitigated by S1/S4 guidance),
- fire-and-forget task accessing `ICorrelationContextAccessor` after scope disposal (documents
  `EmptyCorrelationContext` semantics, ADR 0002).

### T2. CI matrix

Integration tests target `net10.0` only and CI runs `ubuntu-latest` only (ADR 0011). Consider a
matrix (`os: [ubuntu-latest, windows-latest]`, TFM per ADR 0011 policy) - `AsyncLocal` and
`IHeaderDictionary` behavior differences across runtimes are exactly the risk area this library
lives in. Cost/benefit: at minimum add a Windows leg for integration tests.

### T3. Docs sample compilation

The broken configuration sample (U1) recurring suggests a small "docs snippets" test project
referenced from `docs/` samples, compiled in CI (docs paths currently ignored in CI, which is why
U1 went unnoticed). A tiny console project with the documented snippets as source would prevent
drift.

---

## Suggested sequencing

1. **Quick wins (no breaking changes, no new dependencies):** P4 (sync `Send`), A6 (remove unused
   DataAnnotations reference), U1 (docs fix), T1 (tests for the above).
2. **Hardening, strictly opt-in (minor-version friendly):** S3 (registration-time header name
   validation), S5 (sanitize `TraceIdentifier`), S1/A3/A1 (validator guidance + shipped charset
   validator + overloads), S6 (document lazy validation).
3. **Behavior additions (need an ADR each before implementation):** S2 (opt-in invalid-value
   policy, amends ADR 0004), P3 (`Ordinal` equality - next major), A4 (accessor semantics).
4. **Ongoing:** P1/P2 micro-optimizations guarded by benchmarks; T2 CI matrix; U2/U4/U5 docs
   hygiene.
