# Development roadmap

No calendar release dates are committed. Work is ordered by verified dependency gates.

| Phase | Goal | Gate | State |
|---|---|---|---|
| P0 | Repository, license, documentation, toolchain, CI, architecture and truthful capability manifest | Clean baseline, public-safe docs | Done (2026-10-27) |
| P1 | Source-generated metadata and safe SQLite CRUD/transactions PoC | Real SQLite tests, query injection and rollback tests, reproducible Console + Module sample | In progress |
| P2 | First consumable developer handoff | Versioned examples, release notes and verified feature matrix | Planned |
| P3 | PostgreSQL provider | Shared conformance subset verified on both providers | Planned |
| P4 | General-purpose associations, queries, bulk and concurrency | Conformance tests and representative workloads | Planned |
| P5+ | Advanced enterprise features, native interop and optional modules | Separate readiness and capability checks | Planned |

P1 also includes optimistic concurrency, GroupBy and aggregates, a `SPOBaseObject` base class, and policy declarations that fail closed (there is no policy engine yet). The decisions are in [DECISIONS.md](../.claude/docs/DECISIONS.md) (D-29 to D-37); the architecture overview is in [docs/architecture/OVERVIEW.md](architecture/OVERVIEW.md).

MiddleTier and WebAPI are independent future repositories with separate readiness gates. Samples and documentation are part of every milestone, not post-release chores.
