# Tracking contract

**Status: specified, not implemented.**

## Purpose

States how an ObjectSpace tracks scalar values of persistent objects, what the three baselines mean, and how failures
and partial loads must behave. Sources: [OVERVIEW](OVERVIEW.md), [POC acceptance](../POC_ACCEPTANCE.md), issues #3, #5, #6.

## Contract

1. The PoC tracks scalar properties only; original and current value are both kept per tracked scalar.
2. A tracked object is dirty when at least one tracked scalar's current value differs from its original value.
3. Tracking distinguishes three baselines (glossary): **working** (values in memory), **flushed** (values written in the still-open transaction), **committed** (values durably committed).
4. Reading dirty state or original values is transaction-aware: flushed values are not treated as committed until the transaction commits.
5. On commit, the committed baseline advances to the values that were committed.
6. On rollback, the committed baseline does not advance; tracking state afterwards reflects what the database actually holds and what the objects hold in memory, without claiming rolled-back values as committed.
7. After an uncertain commit outcome, tracking does not claim either outcome as certain ([objectspace](objectspace.md) rule 9).
8. Changed audit timestamps are tracked as part of dirty-state detection (PoC requirement).
9. A partial load (only some properties loaded) is explicit: an unloaded property is distinguishable from a loaded null/default value.
10. Plain automatic getters are **not** eligible for partially-loaded persistent entity semantics until instrumentation exists. Until then, partial results are returned as DTO projections, not as tracked entities.
11. Tracking must not rely on runtime reflection on the default path (metadata comes from the generator).

## Not in scope

- Association and collection tracking.
- Change tracking by instrumentation/proxies (does not exist yet; see rule 10).
- Cache invalidation after set-based writes (named in OVERVIEW, not yet specified).

## Open questions

Answers are recorded in [DECISIONS.md](../../.claude/docs/DECISIONS.md); a decided question names its decision.


1. How is "equal" decided for scalar comparison: `Equals`, a metadata-provided comparer, or per-type rules? (decimal, DateTime kinds, byte arrays.) **Decided: D-30.**
2. Does "flushed" exist in the PoC at all, i.e. can writes reach the database before commit other than inside SaveChanges? **Decided: D-30.**
3. After rollback, should in-memory current values stay as the user left them (still dirty), or be reset to the original? **Decided: D-30.**
4. How is a deleted (SoftDeleted) object represented in tracking: a state flag, or a scalar like any other? **Decided: D-35.**
5. Which instrumentation approach will make partial loads of entities possible (generated subclasses, interceptors, other)? Out of the PoC, but affects metadata.
6. What happens to tracked objects after a successful commit on a partially loaded object?
