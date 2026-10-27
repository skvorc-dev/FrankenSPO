# Provider contract

**Status: specified, not implemented.**

## Purpose

States what a database provider must offer, how its capabilities are declared and checked, and what happens when one
is missing. SQLite is first; PostgreSQL follows in milestone P3. Sources: [OVERVIEW](OVERVIEW.md),
[POC acceptance](../POC_ACCEPTANCE.md), `AGENTS.md`, issues #3, #5, #6, D-11. In `src/`, only
`SqliteConnectionFactory` (opens a connection, nothing more) exists.

## Contract

1. Providers implement a provider-neutral contract defined in Core/Abstractions ([modules](modules.md)); Core references no provider.
2. A provider declares its capabilities explicitly (glossary: capability manifest). Capability checks happen when the model is activated, not on first use.
3. A mandatory capability the provider lacks (atomic SaveChanges where required, enforcement of a declared tenant/row/field restriction, a mapping type) is an error at activation. It never degrades to a no-op.
4. Optional capabilities that are missing make the feature unavailable with a clear error.
5. Providers use real parameters for values and metadata-validated identifiers ([query-security](query-security.md)).
6. SQLite is the first provider and the only one in the PoC (P1). Its tests run against a real SQLite database (in-memory or temp file), without mocks.
7. The provider contract and generated metadata must stay compatible with a future PostgreSQL adapter. PostgreSQL is not a PoC feature and is declared `planned` until P3 evidence exists.
8. A provider moves toward `provider-verified` only through conformance tests against the real database (OVERVIEW).
9. A provider reports an uncertain commit outcome as such ([objectspace](objectspace.md) rule 9).
10. `features.json` is the only place where a provider's supported feature status is stated.

## Not in scope

- Provider-specific SQL dialect details beyond what P1 needs.
- Any provider other than SQLite and PostgreSQL.
- Schema migration tooling (PoC only requires deterministic first-time schema initialisation and validation).

## Open questions

Answers are recorded in [DECISIONS.md](../../.claude/docs/DECISIONS.md); a decided question names its decision.


1. What is the shape of the provider contract: interfaces in `Abstractions`, or in `Core`? The sources do not say. **Decided: D-34.**
2. What are the capability names and their granularity? Where do they live: code, `features.json`, or both? **Decided: D-39.**
3. Is the capability manifest in `features.json` (a repository document) the same thing as the runtime capability set, or are they separate? **Decided: D-39.**
4. Who owns schema initialisation and validation: the provider, or Core with provider dialect hooks? **Decided: D-34.**
5. How does a provider know about a connection owned by the caller versus one it opens itself? **Decided: D-29 (P1: the provider opens its own connection).**
6. SQLite specifics for atomicity and isolation (journal mode, locking, multiple connections): not decided. **Decided: D-34.**
