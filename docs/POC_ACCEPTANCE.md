# P1 SQLite PoC — acceptance contract

**Status: specified, not implemented.** This document is the minimum vertical-slice requirement, not a claim about existing code.

## Functional

- Define a mapped customer entity in a reusable `SpoExample.Module` .NET class library.
- Compile generated metadata without runtime reflection as the default mapping route.
- Create and reopen SQLite database; deterministic first-time schema initialization and validation.
- Persist/read/update/SoftDelete through ObjectSpace; UUIDv7 identity for SPOBaseObject; basic declared key support for supported minimal objects.
- Track scalar original/current values, dirty state and changed audit timestamps.
- Query through explicitly supported LINQ subset with parameterized values, metadata-validated identifiers, projections and clear errors for unsupported expressions.
- Commit or roll back changes atomically, preserving honest in-memory tracking state after failures.
- Source-generated and provider contracts remain compatible with future PostgreSQL adapter; **do not** imply PostgreSQL is working yet.

## Security and correctness

- SQL injection regression cases: quotes, boolean tautologies, comments, semicolons, malicious sort names, bulk-like payloads and literal LIKE escaping.
- Provider operations must not use string concatenation for user-supplied values.
- Declared tenant/row/field restrictions are enforced or rejected at model activation; incomplete enforcement cannot silently turn off a mandatory policy.
- Plain automatic getters are not eligible for partially loaded persistent entity semantics until instrumentation exists; use DTO projection initially.
- Audit identity and timestamp policies must be specified and tested before claiming actor-level guarantees.
- Unsupported exact decimal query operations, complex associations and other capability gaps must reject or remain explicitly absent, never silently degrade.

## Acceptance evidence

- Clean .NET 10 build on listed supported CI environments.
- Unit tests plus real SQLite integration tests for CRUD/transaction/security scenarios.
- Working `SpoExample.Module` and `SpoExample.Console` code with documented run instructions.
- Versioned feature manifest updated from `planned` only with corresponding test evidence.
- README, AGENTS, samples and release notes describe exactly the same supported features.

## Decisions that refine this contract

- [D-29](../.claude/docs/DECISIONS.md): ObjectSpace: single-threaded, async-first, one transaction per save
- [D-30](../.claude/docs/DECISIONS.md): Tracking: values stay dirty after a failed save
- [D-31](../.claude/docs/DECISIONS.md): Optimistic concurrency with a version column
- [D-32](../.claude/docs/DECISIONS.md): Queries: a fixed LINQ subset, escaped LIKE
- [D-33](../.claude/docs/DECISIONS.md): Policies: no engine in P1, fail closed
- [D-34](../.claude/docs/DECISIONS.md): Providers and schema: provider-neutral model, dialect DDL
- [D-35](../.claude/docs/DECISIONS.md): Entities: SPOBaseObject with shared columns
- [D-36](../.claude/docs/DECISIONS.md): Modules: explicit registration, no assembly scanning
- [D-37](../.claude/docs/DECISIONS.md): Declaring entities: stable GUID and numbered fields
