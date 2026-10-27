# E1 · SQLite PoC (phase P1)

Milestone **P1 — SQLite PoC**. Contract: `docs/POC_ACCEPTANCE.md`, `docs/architecture/*.md`, decisions D-29…D-36. All cards are `Backlog` until E0 is done;
the main agent completes each card (files, exact acceptance) before moving it to `Ready`.

### S-01 · Generator: model declaration contract and stable metadata IDs
- Issue: #5 · Size: M · Status: Backlog
- Goal: a declared entity compiles to generated metadata (entity and property IDs) without runtime reflection.
- Acceptance:
  - [ ] Generator tests prove metadata output and diagnostics for invalid declarations
  - [ ] `Core` has no reference to the SQLite provider

### S-02 · SQLite schema initialisation and validation
- Issue: #5 · Size: M · Status: Backlog · after S-01
- Goal: Core builds a neutral schema model, the SQLite provider renders DDL; WAL, foreign keys, busy timeout (D-34).
  Settle provider capability names and runtime-vs-manifest (open questions in `providers.md`).
- Goal: create and reopen a database with deterministic first-time schema and validation against metadata.

### S-03 · ObjectSpace CRUD with identity map, `SPOBaseObject` and UUIDv7 keys
- Issue: #5 · Size: M · Status: Backlog · after S-02
- Goal: persist, read, update through ObjectSpace on real SQLite; async-first API with sync twins; single-threaded
  misuse detected (D-29, D-35).

### S-04 · Scalar tracking, audit timestamps, SoftDelete
- Issue: #5 · Size: M · Status: Backlog · after S-03
- Goal: per-type comparers, failed save keeps values dirty (D-30); UTC timestamps via `TimeProvider`; Deleted state,
  `IncludeDeleted()` (D-35).

### S-05 · LINQ core subset with parameters and validated identifiers
- Issue: #6 · Size: M · Status: Backlog · after S-03
- Goal: Where, string StartsWith/EndsWith/Contains, OrderBy/ThenBy, Skip/Take, Select to DTO, First/Single/Count/Any
  (D-32); `SpoQueryNotSupportedException` for the rest.

### S-05b · GroupBy and aggregates
- Issue: #6 · Size: M · Status: Backlog · after S-05
- Goal: GroupBy and Sum/Min/Max/Average with the same parameter and identifier rules (D-32).

### S-06 · Transactions: commit, rollback, uncertain commit, tracking state after failure
- Issue: #6 · Size: M · Status: Backlog · after S-04
- Goal: SaveChanges owns its transaction; `SpoCommitUncertainException` and a discarded ObjectSpace (D-29).

### S-10 · Optimistic concurrency
- Issue: #6 · Size: M · Status: Backlog · after S-06
- Goal: integer version column on `SPOBaseObject`, incremented per update; conflict exception, nothing written (D-31).

### S-11 · Policy declarations fail closed at activation
- Issue: #6 · Size: S · Status: Backlog · after S-01
- Goal: a model declaring a tenant/row/field policy fails when the compiled model is built (D-33); test proves it.

### S-07 · SQL injection regression suite
- Issue: #6 · Size: M · Status: Backlog · after S-05
- Goal: the cases listed in `docs/POC_ACCEPTANCE.md` (quotes, tautologies, comments, semicolons, sort names, LIKE).

### S-08 · `SpoExample.Module` and `SpoExample.Console`
- Issue: #7 · Size: M · Status: Backlog · after S-04
- Goal: compilable sample with run instructions; built in CI.

### S-09 · Capability manifest and docs for the PoC
- Issue: #1 · Size: S · Status: Backlog · last card of E1
- Goal: `features.json`, README, AGENTS.md and samples describe exactly the tested features.
