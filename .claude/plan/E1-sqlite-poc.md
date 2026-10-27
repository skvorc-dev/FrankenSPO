# E1 · SQLite PoC (phase P1)

Milestone **P1 — SQLite PoC**. Contract: `docs/POC_ACCEPTANCE.md`. All cards are `Backlog` until E0 is done;
the main agent completes each card (files, exact acceptance) before moving it to `Ready`.

### S-01 · Generator: model declaration contract and stable metadata IDs
- Issue: #5 · Size: M · Status: Backlog
- Goal: a declared entity compiles to generated metadata (entity and property IDs) without runtime reflection.
- Acceptance:
  - [ ] Generator tests prove metadata output and diagnostics for invalid declarations
  - [ ] `Core` has no reference to the SQLite provider

### S-02 · SQLite schema initialisation and validation
- Issue: #5 · Size: M · Status: Backlog · after S-01
- Goal: create and reopen a database with deterministic first-time schema and validation against metadata.

### S-03 · ObjectSpace CRUD with identity map and UUIDv7 keys
- Issue: #5 · Size: M · Status: Backlog · after S-02
- Goal: persist, read, update through ObjectSpace on real SQLite.

### S-04 · Scalar tracking, audit timestamps, SoftDelete
- Issue: #5 · Size: M · Status: Backlog · after S-03

### S-05 · Bounded LINQ subset with parameters and validated identifiers
- Issue: #6 · Size: L (split before Ready) · Status: Backlog · after S-03

### S-06 · Transactions: commit, rollback, tracking state after failure
- Issue: #6 · Size: M · Status: Backlog · after S-03

### S-07 · SQL injection regression suite
- Issue: #6 · Size: M · Status: Backlog · after S-05
- Goal: the cases listed in `docs/POC_ACCEPTANCE.md` (quotes, tautologies, comments, semicolons, sort names, LIKE).

### S-08 · `SpoExample.Module` and `SpoExample.Console`
- Issue: #7 · Size: M · Status: Backlog · after S-04
- Goal: compilable sample with run instructions; built in CI.

### S-09 · Capability manifest and docs for the PoC
- Issue: #1 · Size: S · Status: Backlog · last card of E1
- Goal: `features.json`, README, AGENTS.md and samples describe exactly the tested features.
