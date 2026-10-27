# E1 · SQLite PoC (phase P1)

Milestone **P1 — SQLite PoC**. Contract: `docs/POC_ACCEPTANCE.md`, `docs/architecture/*.md`, decisions D-29…D-36. All cards are `Backlog` until E0 is done;
the main agent completes each card (files, exact acceptance) before moving it to `Ready`.

### S-01 · Generator: entity declaration and stable metadata IDs
- Issue: #5 · Size: M · Status: In Progress (sonnet-2)
- Goal: a class declared per D-37 compiles to generated, reflection-free metadata with its stable entity ID and
  property numbers; invalid declarations fail the build with clear diagnostics.
- Acceptance:
  - [ ] `Sferix.SPO.Abstractions`: `SpoEntityAttribute(string id)`, `SpoFieldAttribute(int number)`, and immutable
        metadata types `EntityMetadata` (EntityId, CLR name, properties) and `PropertyMetadata` (number, CLR name,
        CLR type, nullable). XML docs per the code style rules.
  - [ ] `Sferix.SPO.Generators` (incremental generator, Roslyn 5.0.0 per D-25): for each `[SpoEntity]` partial class it
        emits a `public static EntityMetadata SpoMetadata { get; }` member in the same partial class
  - [ ] Supported property types: `bool`, `int`, `long`, `decimal`, `double`, `string`, `Guid`, `DateTime`,
        `DateTimeOffset`, `byte[]`, their nullable forms, and enums (stored as their underlying integer)
  - [ ] Diagnostics (errors, IDs `SPO0001`…): class not `partial`; entity ID missing or not a GUID; same entity ID
        twice in a compilation; `[SpoField]` number ≤ 0 or duplicated within the entity; unsupported property type;
        `[SpoField]` on a property without both getter and setter
  - [ ] New test project `tests/Sferix.SPO.Tests.Generators` (xUnit, added to `FrankenSPO.slnx`) runs the generator
        with `CSharpGeneratorDriver`: one test per diagnostic, plus a valid entity whose generated metadata is checked
        field by field; also a compile-and-run test that reads `SpoMetadata` without reflection
  - [ ] `Core` still references no provider; no runtime reflection on the metadata path; build 0 warnings; CI green
  - [ ] `features.json`: `source_generated_metadata` → `tested` only if all of the above pass
- Files: `src/Sferix.SPO.Abstractions/**`, `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.Generators/**`,
  `FrankenSPO.slnx`, `docs/compatibility/features.json`
- Dependency allowed: `Microsoft.CodeAnalysis.CSharp` 5.0.0 in the generator test project (record in THIRD-PARTY-NOTICES)
- Not in scope: `SPOBaseObject` and its columns (S-03), schema (S-02), model revisions, the GUID code fix, tracking code
- Notes: D-25, D-35, D-37, `docs/architecture/modules.md` rule 11.

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
