# ObjectSpace contract

**Status: specified, not implemented.**

## Purpose

States what an ObjectSpace (the unit-of-work session, see the glossary) guarantees: one identity per persistent object,
one place where changes are collected, and one atomic save. Sources: [OVERVIEW](OVERVIEW.md),
[POC acceptance](../POC_ACCEPTANCE.md), issues #3, #5, #6. No ObjectSpace type exists in `src/` yet.

## Contract

1. An ObjectSpace has an identity map: within one ObjectSpace, loading the same entity type and key twice yields the same object instance.
2. Persistent objects loaded or created in an ObjectSpace are tracked by that ObjectSpace; tracking is defined in [tracking](tracking.md).
3. `SPOBaseObject` identity is a UUIDv7; a minimal object without `SPOBaseObject` may use a declared key (PoC scope: basic declared keys only).
4. Create, read, update and SoftDelete go through the ObjectSpace. In the PoC, SoftDelete marks the record; it does not remove the row.
5. Changes are collected in memory and written only by SaveChanges, as one unit of work.
6. Where the provider supports it, SaveChanges is atomic: either all changes in the unit are committed or none are.
7. If a provider cannot provide atomic SaveChanges, that is a mandatory capability deficit and fails closed (see [providers](providers.md)), not a silent non-atomic save.
8. After a failed SaveChanges that rolled back, tracking state is honest: the ObjectSpace does not report changes as committed that were not (see [tracking](tracking.md)).
9. If the commit outcome cannot be determined (for example a connection fails during commit), the failure is reported as uncertain; it is not reported as success or as a clean rollback.
10. Post-commit callbacks are not durable delivery; nothing may rely on them for guarantees.
11. The default mapping path uses source-generated metadata and no runtime reflection ([modules](modules.md)).
12. Queries issued through an ObjectSpace follow [query-security](query-security.md).

## Not in scope

- Associations and complex relationships (rejected or absent in the PoC).
- Set-based (bulk) writes; OVERVIEW requires them to preserve security guarantees and invalidate tracked/cache state, but the model is not yet specified.
- Remote ObjectSpace (MiddleTier) and HTTP endpoints (WebApi): separate repositories.
- Advanced audit/event modules.

## Open questions

Answers are recorded in [DECISIONS.md](../../.claude/docs/DECISIONS.md); a decided question names its decision.


1. Are ObjectSpaces thread-safe, or single-threaded per instance? (Sources are silent.) **Decided: D-29.**
2. Is the identity map keyed by entity type + key, or by root type? What happens to inherited types? **Decided: D-35.**
3. What exactly is "uncertain commit reporting": a dedicated exception type, a result value, or both? Must it carry the changes in the unit? **Decided: D-29.**
4. After an uncertain commit, may the ObjectSpace be used further, or must it be discarded? **Decided: D-29.**
5. Is autonomous commit allowed (a save with no ambient transaction), and can several ObjectSpaces share one transaction? **Decided: D-29.**
6. Do SoftDelete and audit timestamps participate in the same SaveChanges transaction (assumed yes, not stated)? Which actor identity source applies? POC_ACCEPTANCE says this must be specified before actor-level guarantees are claimed. **Decided: D-29, D-35.**
7. Concurrency control (optimistic locking, row versions): not mentioned in any source. In or out of the PoC? **Decided: D-31.**
8. Is an async API (with `CancellationToken`, per the public API rules) part of the first PoC, or sync-first? **Decided: D-29.**
