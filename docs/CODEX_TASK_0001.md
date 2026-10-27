# Codex Task 0001 — smallest working foundation

**Status: ready to assign; not executed.** Related work: issues #2, #3, #4 and umbrella #1.

## Objective

Create a clean .NET 10 solution skeleton that matches the approved modular boundaries, establishes reproducible test infrastructure, and preserves the existing public docs/CI. This task is *not* the entire ORM implementation.

## Target projects

Create only needed initial projects:
- `src/Sferix.SPO.Abstractions` — small external contracts where necessary.
- `src/Sferix.SPO.Core` — ObjectSpace design primitives without vendor-specific references.
- `src/Sferix.SPO.Generators` — Roslyn incremental source generator skeleton and diagnostic tests.
- `src/Sferix.SPO.Provider.SQLite` — SQLite-specific adapter project skeleton (functional work follows).
- `src/Sferix.SPO` — public convenience assembly/package composition only when needed.
- `tests/Sferix.SPO.Tests.Unit` and `tests/Sferix.SPO.Tests.SQLite` — automated test projects.
- `samples/SpoExample.Module` and `samples/SpoExample.Console` only as they become executable.

## Non-goals

No MiddleTier/WebAPI implementation, no optional authentication system, no Rust/native bridge, no high-level association editor, no unsupported placeholder APIs masquerading as working.

## Acceptance

- New solution builds and tests under the tracked CI matrix with no fake success.
- Core does not reference SQLite adapter; generator is a build-time component.
- `AGENTS.md`, architecture and feature matrix match actual implemented state.
- No private or unreleased product information appears in the public repository.
- Separate focused PR with linked issue and clear test results.

Once accepted, follow `docs/POC_ACCEPTANCE.md` to implement the first usable SQLite vertical slice.
