# Development roadmap

No calendar release dates are committed. Work is ordered by verified dependency gates.

| Phase | Goal | Gate |
|---|---|---|
| P0 | Repository, license, documentation, toolchain, CI, architecture and truthful capability manifest | Clean baseline, public-safe docs |
| P1 | Source-generated metadata and safe SQLite CRUD/transactions PoC | Real SQLite tests, query injection and rollback tests, reproducible Console + Module sample |
| P2 | First consumable developer handoff | Versioned examples, release notes and verified feature matrix |
| P3 | PostgreSQL provider | Shared conformance subset verified on both providers |
| P4 | General-purpose associations, queries, bulk and concurrency | Conformance tests and representative workloads |
| P5+ | Advanced enterprise features, native interop and optional modules | Separate readiness and capability checks |

MiddleTier and WebAPI are independent future repositories with separate readiness gates. Samples and documentation are part of every milestone, not post-release chores.
